using System;

using OpenQA.Selenium;

namespace Mememe.Parser.Configurations
{
    [Serializable]
    public class WebDriverConfiguration
    {
        public string? ChromeDriverPath { get; set; }
        public string? ChromeUserAgent { get; set; }

        public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        public string LogPath { get; set; } = "Logs/chromedriver.log";

        public bool SilentMode { get; set; } = false;

        public string Url { get; set; } = "localhost";

        public TimeSpan PageLoadTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ControlWaitTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}