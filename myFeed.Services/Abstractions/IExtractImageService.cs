namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Parses Html for feed content.
    /// </summary>
    public interface IExtractImageService
    {
        /// <summary>
        /// Extracts image url for html content.
        /// </summary>
        string ExtractImage(string html);
    }
}