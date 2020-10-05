using System.IO;

using Mememe.Parser.Configurations;
using Mememe.Parser.Exceptions;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Mememe.Parser
{
    public static class WebDriver
    {
        private static WebDriverConfiguration? _configuration;

        private static IWebDriver? _driver;
        private static IJavaScriptExecutor? _javaScriptExecutor;

        internal static IWebDriver Driver
        {
            get
            {
                if (_driver is null)
                    throw new WebDriverUninitializedException();

                return _driver;
            }
            private set => _driver = value;
        }

        internal static IJavaScriptExecutor JavaScriptExecutor
        {
            get
            {
                if (_javaScriptExecutor is null)
                    throw new WebDriverUninitializedException();

                return _javaScriptExecutor;
            }
            private set => _javaScriptExecutor = value;
        }

        private static WebDriverConfiguration Configuration
        {
            get
            {
                if (_configuration is null)
                    throw new WebDriverUninitializedException();

                return _configuration;
            }
            set => _configuration = value;
        }

        #region Management

        public static void Start(WebDriverConfiguration configuration)
        {
            Configuration = configuration;

            var service = InitializeDriverService(configuration);
            var options = InitializeDriverOptions(configuration);

            Driver = new ChromeDriver(service, options);
            JavaScriptExecutor = (IJavaScriptExecutor) Driver;

            var timeouts = Driver.Manage().Timeouts();
            timeouts.PageLoad = Configuration.PageLoadTimeout;
            timeouts.ImplicitWait = Configuration.ControlWaitTimeout;

            Driver.Url = configuration.Url;
        }

        public static void Stop()
        {
            Driver.Quit();
            Driver.Dispose();
        }

        private static ChromeDriverService InitializeDriverService(WebDriverConfiguration configuration)
        {
            configuration.ChromeDriverPath ??= Directory.GetCurrentDirectory();

            var service = ChromeDriverService.CreateDefaultService(configuration.ChromeDriverPath);
            service.LogPath = configuration.LogPath;

            return service;
        }

        private static ChromeOptions InitializeDriverOptions(WebDriverConfiguration configuration)
        {
            var options = new ChromeOptions();
            options.AddArgument("-–incognito");
            options.AddArgument("--start-fullscreen");
            options.SetLoggingPreference(LogType.Driver, configuration.LogLevel);

            if (configuration.ChromeUserAgent != null)
                options.AddArgument($"--user-agent={configuration.ChromeUserAgent}");

            if (configuration.SilentMode)
                options.AddArgument("--headless");

            return options;
        }

        #endregion

        #region Input

        public static void Click(Control control)
        {
            if (!IsExists(control))
                throw new ControlDoesntExistException(control);

            control.Element.Click();
        }

        public static void Input(Control control, string text)
        {
            if (!IsExists(control))
                throw new ControlDoesntExistException(control);

            control.Element.SendKeys(text);
        }

        #endregion

        #region Output

        public static string GetText(Control control)
        {
            if (!IsExists(control))
                throw new ControlDoesntExistException(control);

            return control.Element.Text;
        }

        public static string? GetAttributeValue(Control control, string attributeName)
        {
            if (!IsExists(control))
                throw new ControlDoesntExistException(control);

            return control.Element.GetAttribute(attributeName);
        }

        public static int GetElementCount(Control control) => control.Elements.Count;

        public static bool IsExists(Control control)
        {
            try
            {
                JavaScriptExecutor.ExecuteScript("arguments[0].scrollIntoView(false)", control.Element);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}