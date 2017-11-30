using System;
using System.Text.RegularExpressions;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class RegexImageService : IImageService
    {
        private const string Pattern = "<img.*?src=[\"'](http.+?)[\"'].*?>";
        private const RegexOptions Options = RegexOptions.Compiled 
            | RegexOptions.CultureInvariant 
            | RegexOptions.IgnoreCase
            | RegexOptions.Multiline;

        private static readonly Regex Matcher = new Regex(Pattern, Options);

        public string ExtractImageUri(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;

            var match = Matcher.Match(html);
            if (!match.Success) return null;

            var imageUrl = match.Groups[1].Value;
            return Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute) ? imageUrl : null;
        }
    }
}