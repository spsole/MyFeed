using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface ISearchService
    {
        Task<FeedlyRoot> SearchAsync(string query);
    }
}