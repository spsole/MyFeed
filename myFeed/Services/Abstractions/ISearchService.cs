using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Services.Abstractions
{
    public interface ISearchService
    {
        Task<FeedlyRoot> SearchAsync(string query);
    }
}