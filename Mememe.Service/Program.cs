using System;

using Mememe.Parser;
using Mememe.Service.Configurations;
using Mememe.Service.Database;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Mememe.Service
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                CreateHost(args).Run();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHost CreateHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .UseSerilog((context, _) =>
                {
                    Log.Logger = new LoggerConfiguration()
                       .ReadFrom.Configuration(context.Configuration)
                       .CreateLogger();
                }, true)
               .UseSystemd()
               .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(_ =>
                    {
                        var parsingConfiguration = new WebDriver.Configuration();
                        hostContext.Configuration.Bind("Parsing", parsingConfiguration);

                        return parsingConfiguration;
                    });

                    services.AddSingleton(_ =>
                    {
                        var applicationConfiguration = new ApplicationConfiguration();
                        hostContext.Configuration.Bind("Application", applicationConfiguration);

                        return applicationConfiguration;
                    });

                    services.AddSingleton(_ =>
                    {
                        var mongoConfiguration = new MongoConfiguration();
                        hostContext.Configuration.Bind("Mongo", mongoConfiguration);

                        return mongoConfiguration;
                    });

                    services
                       .AddHostedService<ParserService>()
                       .AddSingleton<IMongo>(provider => new Mongo(provider));
                })
               .Build();
    }
}