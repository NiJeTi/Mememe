using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQA.Selenium;

namespace Mememe.Parser
{
    public class Control
    {
        private readonly Func<string, By> _selectorParser;

        public Control(string selector, SelectorType selectorType = SelectorType.Css, Control? parent = null)
        {
            Selector = selector;
            SelectorType = selectorType;

            Parent = parent;

            _selectorParser = selectorType switch
            {
                SelectorType.Css => By.CssSelector,
                SelectorType.Xpath => By.XPath,
                _ => throw new ArgumentOutOfRangeException(nameof(selectorType), selectorType,
                    $"Unknown type of selector \"{selectorType}\"")
            };
        }

        public string Selector { get; }
        public SelectorType SelectorType { get; }

        public Control? Parent { get; }

        internal IWebElement Element => Elements.First();

        internal IReadOnlyCollection<IWebElement> Elements =>
            Parent is null
                ? WebDriver.Driver.FindElements(_selectorParser(Selector))
                : Parent.Element.FindElements(_selectorParser(Selector));

        public override string ToString()
        {
            var stringBuilder = new StringBuilder($"Control with {SelectorType} selector \"{Selector}\"");

            if (Parent != null)
                stringBuilder.Append($" and parent: {Parent}");

            return stringBuilder.ToString();
        }
    }
}