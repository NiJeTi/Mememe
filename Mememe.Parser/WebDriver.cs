using System;
using System.IO;
using System.Threading;

using Mememe.Parser.Exceptions;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Mememe.Parser
{
    public static class WebDriver
    {
        private static Configuration _configuration = new Configuration();
        private static IWebDriver? _driver;

        internal static IWebDriver Driver
        {
            get
            {
                if (_driver == null)
                    throw new WebDriverUninitializedException();

                return _driver;
            }

            private set => _driver = value;
        }

        public static bool IsReady { get; private set; } = false;

        [Serializable]
        public class Configuration
        {
            public string? ChromeDriverPath { get; set; }
            public string LogPath { get; set; } = "Logs/chromedriver.log";

            public bool SilentMode { get; set; } = false;

            public string Url { get; set; } = "localhost";

            public TimeSpan PageLoadTimeout { get; set; } = TimeSpan.FromSeconds(10);
            public TimeSpan ControlWaitTimeout { get; set; } = TimeSpan.FromSeconds(5);
        }

        #region Management

        public static void Initialize(Configuration configuration)
        {
            _configuration = configuration;

            string driverPath = string.IsNullOrEmpty(configuration.ChromeDriverPath)
                ? Directory.GetCurrentDirectory()
                : configuration.ChromeDriverPath;

            var service = ChromeDriverService.CreateDefaultService(driverPath);
            service.LogPath = configuration.LogPath;

            var options = new ChromeOptions();
            options.AddArgument("-–incognito");
            options.AddArgument("--start-fullscreen");
            options.SetLoggingPreference(LogType.Browser, LogLevel.Debug);

            if (configuration.SilentMode)
                options.AddArgument("--headless");

            Driver = new ChromeDriver(service, options) { Url = _configuration.Url };
            IsReady = true;

            new WebDriverWait(Driver, configuration.PageLoadTimeout)
               .Until(driver => ((IJavaScriptExecutor) driver)
                   .ExecuteScript("return document.readyState")
                   .Equals("complete"));
        }

        public static void Dispose()
        {
            Driver.Close();
            Driver.Dispose();

            IsReady = false;
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

        public static bool IsExists(Control control) =>
            SpinWait.SpinUntil(() =>
                {
                    try
                    {
                        ((IJavaScriptExecutor) Driver)
                           .ExecuteScript("arguments[0].scrollIntoView(true)", control.Element);

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }, _configuration.ControlWaitTimeout
            );

        #endregion
    }
}