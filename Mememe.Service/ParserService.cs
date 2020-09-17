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
using Microsoft.Extensions.Logging;

namespace Mememe.Service
{
    public class ParserService : BackgroundService
    {
        private int _articleCounter;
        private readonly ILogger<ParserService> _logger;

        private readonly IMongo _mongo;

        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly WebDriver.Configuration _parsingConfiguration;

        private DateTime _lastRun = DateTime.MinValue;

        public ParserService(ILogger<ParserService> logger, IMongo mongo,
            ApplicationConfiguration applicationConfiguration, WebDriver.Configuration parsingConfiguration)
        {
            _logger = logger;

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

                foreach (var article in Parse())
                    await Upload(article);
            }
        }

        private IEnumerable<Article> Parse()
        {
            WebDriver.Initialize(_parsingConfiguration);
            _logger.LogInformation("Initialized web-driver");

            for (var i = 0; i < _applicationConfiguration.ContentAmount; i++)
            {
                _logger.LogDebug($"Begin parsing article {++_articleCounter}");
                
                string title = MainPageScenarios.GetTitle(i);

                var article = new Article(title)
                {
                    Image = MainPageScenarios.GetImage(i),
                    Video = MainPageScenarios.GetVideo(i)
                };
                
                _logger.LogInformation($"Parsed article {_articleCounter} ({title})");

                yield return article;
            }

            WebDriver.Dispose();
            _logger.LogInformation("Disposed web-driver");
        }

        private async Task Upload(Article article)
        {
            _logger.LogDebug($"Begin uploading \"{article.Title}\" article");
            
            await _mongo.UploadArticle(article);
            
            _logger.LogInformation($"Uploaded \"{article.Title}\" article");
        }
    }
}