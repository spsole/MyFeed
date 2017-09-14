using System;
using myFeed.Services.Abstractions;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace myFeed.Services.Implementations
{
    public class AngleSharpHtmlService : IHtmlService
    {
        public string ExtractImage(string html)
        {
            var parser = new HtmlParser();
            var element = parser.Parse(html).QuerySelector("img");
            if (element == null) return null;
            
            var source = ((IHtmlImageElement) element).Source;
            return Uri.IsWellFormedUriString(source, UriKind.Absolute) ? source : null;
        }
    }
}