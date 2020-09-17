using System;

namespace Mememe.Parser.Exceptions
{
    public class WebDriverUninitializedException : NullReferenceException
    {
        public WebDriverUninitializedException() : base("Web-driver hasn't been initialized") { }
    }
}