using System;

namespace Mememe.NineGag.Models
{
    [Serializable]
    public class Article
    {
        public Article(string title)
        {
            Title = title;
        }

        public string Title { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
    }
}