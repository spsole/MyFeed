using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface ISearchService
    {
        Task<FeedlyRoot> Search(string query);
    }
}