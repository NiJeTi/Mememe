using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.NineGag.Scenarios.MainPage;
using Mememe.Parser;
using Mememe.Parser.Exceptions;
using Mememe.Service.Configurations;
using Mememe.Service.Database;

using Microsoft.Extensions.Hosting;

using Serilog;

using Timer = System.Timers.Timer;

namespace Mememe.Service.Services
{
    public class ParserService : BackgroundService
    {
        private readonly IMongo _mongo;

        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly WebDriver.Configuration _parsingConfiguration;

        private readonly Timer _triggerTimer;

        public ParserService(IMongo mongo,
            ApplicationConfiguration applicationConfiguration, WebDriver.Configuration parsingConfiguration)
        {
            _mongo = mongo;

            _applicationConfiguration = applicationConfiguration;
            _parsingConfiguration = parsingConfiguration;

            _triggerTimer = new Timer(applicationConfiguration.RepeatEvery.TotalMilliseconds);
            _triggerTimer.Elapsed += (_, __) => Trigger();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _triggerTimer.Start();
            Trigger();

            await Task.Yield();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _triggerTimer.Stop();

            await Task.Yield();
        }

        public override void Dispose()
        {
            _triggerTimer.Dispose();

            if (WebDriver.State != WebDriverState.Disposed)
                WebDriver.Dispose();

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) => await Task.Yield();

        private void Trigger()
        {
            int contentAmount = _applicationConfiguration.ContentAmount;

            Log.Information($"Triggered parsing of {contentAmount} articles");

            var uploadTasks = Parse().Where(a => a != null).Select(Upload!).ToArray();
            Task.WaitAll(uploadTasks);

            int parsedContentAmount = uploadTasks.Length;

            if (parsedContentAmount > contentAmount / 2)
                Log.Information($"{parsedContentAmount} articles were parsed and uploaded");
            else if (parsedContentAmount <= contentAmount / 2)
                Log.Warning($"Only {parsedContentAmount} articles were parsed and uploaded");
            else if (parsedContentAmount == 0)
                Log.Error("No articles were parsed and uploaded");
        }

        private IEnumerable<Article?> Parse()
        {
            if (WebDriver.State != WebDriverState.Ready)
            {
                WebDriver.Initialize(_parsingConfiguration);
                Log.Debug("Initialized web-driver");
            }

            for (var i = 0; i < _applicationConfiguration.ContentAmount; i++)
            {
                Log.Debug($"Began parsing article {i + 1}");

                Article? article = null;

                try
                {
                    string title = MainPageScenarios.GetTitle(i);

                    article = new Article(title)
                    {
                        Image = MainPageScenarios.GetImage(i),
                        Video = MainPageScenarios.GetVideo(i)
                    };

                    Log.Debug($"Parsed article {i + 1} (\"{title}\")");
                }
                catch (ControlDoesntExistException exception)
                {
                    var logBuilder = new StringBuilder();

                    logBuilder.AppendLine($"Error while parsing article {i + 1}")
                       .Append(exception);

                    Log.Warning(logBuilder.ToString());
                }

                yield return article;
            }

            if (WebDriver.State is WebDriverState.Ready)
            {
                WebDriver.Dispose();
                Log.Debug("Disposed web-driver");
            }
        }

        private async Task Upload(Article article)
        {
            Log.Debug($"Began uploading \"{article.Title}\" article");

            await _mongo.UploadArticle(article);

            Log.Debug($"Uploaded \"{article.Title}\" article");
        }
    }
}