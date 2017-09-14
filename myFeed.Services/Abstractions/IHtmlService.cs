namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Parses Html for feed content.
    /// </summary>
    public interface IHtmlService
    {
        /// <summary>
        /// Extracts image url for html content.
        /// </summary>
        /// <param name="html">Content to parse.</param>
        string ExtractImage(string html);
    }
}