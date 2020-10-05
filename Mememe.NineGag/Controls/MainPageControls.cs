using Mememe.Parser;

namespace Mememe.NineGag.Controls
{
    public class MainPageControls
    {
        public MainPageControls()
        {
            HotLink = new Control("[href=\"/hot\"]");
            TrendingLink = new Control("[href=\"/trending\"]");
            FreshLink = new Control("[href=\"/fresh\"]");

            ArticlesSectionForm = new Control("section#list-view-2");

            ArticleForm = new Control("article[id]", parent: ArticlesSectionForm);
        }

        public Control HotLink { get; }
        public Control TrendingLink { get; }
        public Control FreshLink { get; }

        public Control ArticlesSectionForm { get; }

        public Control ArticleForm { get; }

        public Control GetArticleByIndex(int index) =>
            new Control($"(//article[@id])[{index + 1}]", SelectorType.Xpath, ArticlesSectionForm);

        public Control GetArticleTitle(Control article) => new Control("h1", parent: article);

        public Control GetArticleImage(Control article) => new Control("img", parent: GetArticleContentForm(article));

        public Control GetArticleVideo(Control article) =>
            new Control("video source[type=\"video/mp4\"]", parent: GetArticleContentForm(article));

        private Control GetArticleContentForm(Control article) => new Control(".post-container", parent: article);
    }
}