using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions 
{
    public interface IFeedFetchService
    {
        Task<IEnumerable<Article>> FetchAsync(string uri);
    }
}