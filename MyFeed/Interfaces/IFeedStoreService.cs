using System.Collections.Generic;
using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Interfaces
{
    public interface IFeedStoreService
    {
        Task<IEnumerable<Article>> Load(IEnumerable<Channel> channels);
    }
}