using System;
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

        #region Management

        public static void Initialize(Configuration configuration)
        {
            _configuration = configuration;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("-–incognito");
            chromeOptions.AddArgument("--start-fullscreen");

            if (configuration.SilentMode)
                chromeOptions.AddArgument("--headless");

            Driver = new ChromeDriver(configuration.ChromeDriverLocation, chromeOptions) { Url = _configuration.Url };

            new WebDriverWait(Driver, configuration.PageLoadTimeout)
               .Until(driver => ((IJavaScriptExecutor) driver)
                   .ExecuteScript("return document.readyState")
                   .Equals("complete"));
        }

        public static void Dispose()
        {
            Driver.Close();
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
                           .ExecuteScript("arguments[0].scrollIntoView(false)", control.Element);

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }, _configuration.ControlWaitTimeout
            );

        #endregion

        public class Configuration
        {
            public string ChromeDriverLocation { get; set; } =
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            public bool SilentMode { get; set; } = false;

            public string Url { get; set; } = "localhost";

            public TimeSpan PageLoadTimeout { get; set; } = TimeSpan.FromSeconds(10);
            public TimeSpan ControlWaitTimeout { get; set; } = TimeSpan.FromSeconds(5);
        }
    }
}