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
        Task<SearchRootEntity> Search(string query);
    }
}