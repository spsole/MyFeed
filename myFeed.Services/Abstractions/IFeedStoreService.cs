using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFeedStoreService
    {
        Task<Tuple<IEnumerable<Exception>, IEnumerable<Article>>> LoadAsync(IEnumerable<Channel> channels);
    }
}