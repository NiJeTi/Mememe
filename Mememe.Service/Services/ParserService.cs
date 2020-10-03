using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.NineGag.Scenarios.MainPage;
using Mememe.Parser;
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

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) => await Task.Yield();

        private void Trigger()
        {
            Log.Information("Triggered parsing");

            var uploadTasks = Parse().Select(Upload).ToArray();
            Task.WaitAll(uploadTasks);

            Log.Information($"Successfully parsed and uploaded {_applicationConfiguration.ContentAmount} articles");
        }

        private IEnumerable<Article> Parse()
        {
            WebDriver.Initialize(_parsingConfiguration);
            Log.Debug("Initialized web-driver");

            for (var i = 0; i < _applicationConfiguration.ContentAmount; i++)
            {
                Log.Debug($"Began parsing article {i + 1}");

                string title = MainPageScenarios.GetTitle(i);

                var article = new Article(title)
                {
                    Image = MainPageScenarios.GetImage(i),
                    Video = MainPageScenarios.GetVideo(i)
                };

                Log.Debug($"Parsed article {i + 1} (\"{title}\")");

                yield return article;
            }

            WebDriver.Dispose();
            Log.Debug("Disposed web-driver");
        }

        private async Task Upload(Article article)
        {
            Log.Debug($"Began uploading \"{article.Title}\" article");

            await _mongo.UploadArticle(article);

            Log.Debug($"Uploaded \"{article.Title}\" article");
        }
    }
}