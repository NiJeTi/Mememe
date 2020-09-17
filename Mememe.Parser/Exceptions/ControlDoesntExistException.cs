using OpenQA.Selenium;

namespace Mememe.Parser.Exceptions
{
    public class ControlDoesntExistException : NoSuchElementException
    {
        public ControlDoesntExistException(Control control) : base($"{control} doesn't exist") { }
    }
}