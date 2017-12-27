using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFeedStoreService
    {
        Task<IEnumerable<Article>> LoadAsync(IEnumerable<Channel> channels);
    }
}