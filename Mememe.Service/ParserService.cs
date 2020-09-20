using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.NineGag.Scenarios.MainPage;
using Mememe.Parser;
using Mememe.Service.Configurations;
using Mememe.Service.Database;

using Microsoft.Extensions.Hosting;

using Serilog;

namespace Mememe.Service
{
    public class ParserService : BackgroundService
    {
        private static int _parsedArticles;

        private readonly IMongo _mongo;

        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly WebDriver.Configuration _parsingConfiguration;

        private DateTime _lastRun = DateTime.MinValue;

        public ParserService(IMongo mongo,
            ApplicationConfiguration applicationConfiguration, WebDriver.Configuration parsingConfiguration)
        {
            _mongo = mongo;

            _applicationConfiguration = applicationConfiguration;
            _parsingConfiguration = parsingConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (DateTime.Now <= _lastRun + _applicationConfiguration.RepeatEvery)
                    continue;

                _lastRun = DateTime.Now;

                var uploadTasks = new List<Task>(_applicationConfiguration.ContentAmount);

                foreach (var article in Parse(_parsingConfiguration, _applicationConfiguration.ContentAmount))
                {
                    var uploadTask = Upload(article);
                    uploadTask.Start();

                    uploadTasks.Add(uploadTask);
                }

                Task.WaitAll(uploadTasks.ToArray());

                Log.Information("Successfully completed parsing and uploading");
            }
        }

        private static IEnumerable<Article> Parse(WebDriver.Configuration parsingConfiguration, int contentAmount)
        {
            WebDriver.Initialize(parsingConfiguration);
            Log.Debug("Initialized web-driver");

            for (var i = 0; i < contentAmount; i++)
            {
                Log.Debug($"Began parsing article {++_parsedArticles}");

                string title = MainPageScenarios.GetTitle(i);

                var article = new Article(title)
                {
                    Image = MainPageScenarios.GetImage(i),
                    Video = MainPageScenarios.GetVideo(i)
                };

                Log.Debug($"Parsed article {_parsedArticles} ({title})");

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