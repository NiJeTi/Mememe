using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.Service.Configurations;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace Mememe.Service.Database
{
    public class Mongo : IMongo
    {
        private readonly IMongoDatabase _database;

        private IMongoCollection<Article> _currentCollection;
        private DateTime _currentCollectionDate;

        public Mongo(IServiceProvider serviceProvider)
        {
            var mongoConfig = serviceProvider.GetService<MongoConfiguration>();

            string connectionString = BuildConnectionString(mongoConfig);
            
            var client = new MongoClient(connectionString);
            
            _database = client.GetDatabase(mongoConfig.Database);
        }

        private IMongoCollection<Article> CurrentCollection
        {
            get
            {
                if (_currentCollectionDate == DateTime.Today)
                    return _currentCollection;
                
                var currentCollectionName = DateTime.Today.ToString("s");

                IMongoCollection<Article> currentCollection;
            
                if (IsCollectionExists(currentCollectionName, _database.ListCollectionNames()))
                {
                    currentCollection = _database.GetCollection<Article>(currentCollectionName);
                }
                else
                {
                    _database.CreateCollection(currentCollectionName);

                    currentCollection = _database.GetCollection<Article>(currentCollectionName);
                }

                _currentCollectionDate = DateTime.Today;
                _currentCollection = currentCollection;

                return currentCollection;
            }
        }

        public async Task UploadArticle(Article article)
        {
            var collection = CurrentCollection;

            await collection.InsertOneAsync(article);
        }

        private static bool IsCollectionExists(string collectionName, IAsyncCursor<string> collectionList)
        {
            while (collectionList.MoveNext())
                if (collectionList.Current.Contains(collectionName))
                    return true;

            return false;
        }
        
        private static string BuildConnectionString(MongoConfiguration configuration)
        {
            var builder = new StringBuilder("mongodb://");

            builder
               .Append(configuration.Username)
               .Append(':')
               .Append(configuration.Password)
               .Append('@')
               .Append(configuration.Host)
               .Append(':')
               .Append(configuration.Port)
               .Append('/')
               .Append(configuration.Database)
               .Append("?authMechanism=")
               .Append(configuration.AuthMechanism);

            return builder.ToString();
        }
    }
}