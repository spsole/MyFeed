using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces 
{
    public interface IFeedFetchService
    {
        Task<IEnumerable<Article>> FetchAsync(string uri);
    }
}