using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Mememe.NineGag.Models;
using Mememe.Service.Configurations;
using Mememe.Service.Models;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

using Serilog;

namespace Mememe.Service.Database
{
    public class Mongo : IDatabase
    {
        private const string ArticlesCollection = "articles";
        private const string ImagesCollections = "images";
        private const string VideosCollection = "videos";

        private readonly IMongoDatabase _database;

        private readonly IMongoCollection<StoredArticle> _articlesCollection;
        private readonly GridFSBucket _images;
        private readonly GridFSBucket _videos;

        public Mongo(MongoConfiguration configuration)
        {
            string connectionString = BuildConnectionString(configuration);

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(configuration.Database);

            if (!IsCollectionExistsInDatabase(ArticlesCollection))
                _database.CreateCollection(ArticlesCollection);

            _articlesCollection = _database.GetCollection<StoredArticle>(ArticlesCollection);

            var imagesOptions = new GridFSBucketOptions { BucketName = ImagesCollections };
            var videosOptions = new GridFSBucketOptions { BucketName = VideosCollection };

            _images = new GridFSBucket(_database, imagesOptions);
            _videos = new GridFSBucket(_database, videosOptions);
        }

        public async Task<bool> UploadArticle(Article article)
        {
            ObjectId contentId;

            if (article.Image != null)
            {
                contentId = await UploadImage(article.Title, article.Image);
            }
            else if (article.Video != null)
            {
                contentId = await UploadVideo(article.Title, article.Video);
            }
            else
            {
                Log.Warning($"No content in parsed article \"{article}\"");

                return false;
            }

            var databaseArticle = new StoredArticle(article.Title) { ContentId = contentId };
            await _articlesCollection.InsertOneAsync(databaseArticle);

            return true;
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

        private bool IsCollectionExistsInDatabase(string collection)
        {
            var collectionList = _database.ListCollectionNames();

            while (collectionList.MoveNext())
                if (collectionList.Current.Contains(collection))
                    return true;

            return false;
        }

        private async Task<ObjectId> UploadImage(string title, string link)
        {
            using var downloadClient = new WebClient();
            await using var downloadStream = await downloadClient.OpenReadTaskAsync(link);

            return await _images.UploadFromStreamAsync(title, downloadStream);
        }

        private async Task<ObjectId> UploadVideo(string title, string link)
        {
            using var downloadClient = new WebClient();
            await using var downloadStream = await downloadClient.OpenReadTaskAsync(link);

            return await _videos.UploadFromStreamAsync(title, downloadStream);
        }
    }
}