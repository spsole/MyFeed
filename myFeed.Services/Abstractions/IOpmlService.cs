using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// OPML manager. Contains methods to work with 
    /// OPML format and with ISourcesService.
    /// </summary>
    public interface IOpmlService
    {
        /// <summary>
        /// Imports feeds from OPML format.
        /// </summary>
        Task ImportOpmlFeeds();

        /// <summary>
        /// Exports feeds to popular OPML format.
        /// </summary>
        Task ExportOpmlFeeds();
    }
}