using System.Runtime.InteropServices;

using Mememe.Parser;
using Mememe.Service.Configurations;
using Mememe.Service.Database;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mememe.Service
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var hostBuilder = Host
               .CreateDefaultBuilder(args)
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
                });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                hostBuilder = hostBuilder.UseSystemd();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                hostBuilder = hostBuilder.UseWindowsService();

            hostBuilder
               .Build()
               .Run();
        }
    }
}