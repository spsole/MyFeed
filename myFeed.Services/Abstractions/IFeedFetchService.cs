using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Entities.Local;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Retrieves single feed.
    /// </summary>
    public interface IFeedFetchService
    {
        /// <summary>
        /// Fetches feed using passed uri and returns article entities
        /// built from this feed with an exception if it has occured.
        /// </summary>
        Task<(Exception, IEnumerable<ArticleEntity>)> FetchAsync(string uri);
    }
}