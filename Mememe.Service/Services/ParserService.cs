using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.NineGag.Scenarios;
using Mememe.Parser;
using Mememe.Parser.Configurations;
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
        private readonly IDatabase _database;

        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly WebDriverConfiguration _parserConfiguration;

        private readonly Timer _triggerTimer;

        public ParserService(IDatabase database,
            ApplicationConfiguration applicationConfiguration, WebDriverConfiguration parserConfiguration)
        {
            _database = database;

            _applicationConfiguration = applicationConfiguration;
            _parserConfiguration = parserConfiguration;

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
            WebDriver.Stop();
            Log.Debug("Stopped web-driver due to service stop");
            
            _triggerTimer.Dispose();

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) => await Task.Yield();

        private void Trigger()
        {
            int expectedAmount = _applicationConfiguration.ContentAmount;

            Log.Information($"Triggered parsing of {expectedAmount} articles");

            var uploadTasks = Parse()
               .Where(a => a != null)
               .Select(Upload!).ToArray();

            Task.WaitAll(uploadTasks);

            int actualAmount = uploadTasks.Length;

            if (actualAmount > expectedAmount / 2)
                Log.Information($"{actualAmount} articles has been successfully processed");
            else if (actualAmount <= expectedAmount / 2)
                Log.Warning($"Only {actualAmount} articles has been successfully processed");
            else if (actualAmount == 0)
                Log.Error("No articles has been successfully processed");
        }

        private IEnumerable<Article?> Parse()
        {
            WebDriver.Start(_parserConfiguration);
            Log.Debug("Started web-driver");

            MainPageScenarios.OpenFreshSection();

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

                    Log.Debug($"Article {i + 1} (\"{article}\") has been parsed");
                }
                catch (ControlDoesntExistException exception)
                {
                    var logBuilder = new StringBuilder();

                    logBuilder.AppendLine($"Article {i + 1} hasn't been parsed")
                       .Append(exception);

                    Log.Warning(logBuilder.ToString());
                }

                yield return article;
            }

            WebDriver.Stop();
            Log.Debug("Stopped web-driver");
        }

        private async Task Upload(Article article)
        {
            Log.Debug($"Began uploading \"{article}\" article");

            bool result = await _database.UploadArticle(article);

            Log.Debug(result
                ? $"Article \"{article}\" has been uploaded"
                : $"Article \"{article}\" hasn't been uploaded");
        }
    }
}