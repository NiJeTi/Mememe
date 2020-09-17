namespace Mememe.NineGag.Models
{
    public class Article
    {
        public Article(string title)
        {
            Title = title;
        }

        public string Title { get; }
        public string? Image { get; set; }
        public string? Video { get; set; }
    }
}