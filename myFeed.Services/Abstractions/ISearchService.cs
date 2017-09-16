using System.Threading.Tasks;
using myFeed.Entities.Feedly;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Searches for feeds.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Performs search.
        /// </summary>
        /// <param name="query">Search query.</param>
        Task<SearchRootEntity> Search(string query);
    }
}