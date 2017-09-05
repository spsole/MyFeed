using System;
using System.Text.RegularExpressions;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations {
    public class HtmlParsingService : IHtmlParsingService {
        public string ExtractImageUrl(string html) {

            // Match first image tag.
            if (string.IsNullOrWhiteSpace(html)) return null;
            var match = Regex.Match(html, @"<img(.*?)>", RegexOptions.Compiled);
            if (!match.Success || match.Groups[1]?.Value == null) return null;

            // Match image tag src.
            var image = match.Groups[1].Value;
            var src = Regex.Match(image, @"src=\""(.*?)\""", RegexOptions.Compiled);
            if (!src.Success || src.Groups[1]?.Value == null) return null;

            // Check if src uri is valid.
            var urlString = src.Groups[1].Value;
            if (Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                return urlString;

            // Hack that works for certain websites.
            if (urlString.Length > 3 && urlString[0] == '/' &&
                Uri.IsWellFormedUriString($"http:{urlString}", UriKind.Absolute))
                return $"http:{urlString}";
            return null;
        }
    }
}