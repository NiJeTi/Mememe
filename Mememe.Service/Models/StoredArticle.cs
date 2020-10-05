using System;

using MongoDB.Bson;

namespace Mememe.Service.Models
{
    [Serializable]
    public class StoredArticle
    {
        public StoredArticle(string title)
        {
            Title = title;
        }

        public string Title { get; set; }

        public DateTime UploadTime { get; set; } = DateTime.Now;
        public ObjectId ContentId { get; set; }
    }
}