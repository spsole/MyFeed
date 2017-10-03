using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities.Local;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Retrieves feeds and saves them locally.
    /// </summary>
    public interface IFeedStoreService
    {
        /// <summary>
        /// Retrieves single feed for passed multiple sources. Also 
        /// returns exceptions or an empty sequence if none occured.
        /// </summary>
        Task<(IEnumerable<Exception>, 
              IOrderedEnumerable<ArticleEntity>)> GetAsync(
              IEnumerable<SourceEntity> entities);
    }
}