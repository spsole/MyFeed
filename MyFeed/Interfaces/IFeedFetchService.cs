using System.Collections.Generic;
using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Interfaces 
{
    public interface IFeedFetchService
    {
        Task<IEnumerable<Article>> Fetch(string uri);
    }
}