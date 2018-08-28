using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Interfaces
{
    public interface ISearchService
    {
        Task<FeedlyRoot> Search(string query);
    }
}