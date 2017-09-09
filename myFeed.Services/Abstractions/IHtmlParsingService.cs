namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Parses Html for feed content.
    /// </summary>
    public interface IHtmlParsingService
    {
        /// <summary>
        /// Extracts image url for html content.
        /// </summary>
        /// <param name="html">Content to parse.</param>
        string ExtractImageUrl(string html);
    }
}