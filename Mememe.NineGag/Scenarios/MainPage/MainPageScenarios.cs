using Mememe.NineGag.Controls;
using Mememe.NineGag.Controls.MainPage;
using Mememe.Parser;

namespace Mememe.NineGag.Scenarios.MainPage
{
    public static class MainPageScenarios
    {
        private static readonly MainPageControls Controls = ControlsRepository.MainPageControls;

        public static string GetTitle(int articleIndex)
        {
            var article = Controls.GetArticleByIndex(articleIndex);

            return WebDriver.GetText(Controls.GetArticleTitle(article));
        }

        public static string? GetImage(int articleIndex)
        {
            var article = Controls.GetArticleByIndex(articleIndex);
            var articleImage = Controls.GetArticleImage(article);

            return WebDriver.IsExists(articleImage) ? WebDriver.GetAttributeValue(articleImage, "src") : null;
        }

        public static string? GetVideo(int articleIndex)
        {
            var article = Controls.GetArticleByIndex(articleIndex);
            var articleVideo = Controls.GetArticleVideo(article);

            return WebDriver.IsExists(articleVideo) ? WebDriver.GetAttributeValue(articleVideo, "src") : null;
        }
    }
}