using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFeedStoreService
    {
        Task<(IEnumerable<Exception>, IOrderedEnumerable<Article>)> LoadAsync(IEnumerable<Channel> channels);
    }
}