using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities.Local;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Retrieves feeds and saves them locally.
    /// </summary>
    public interface IFeedService
    {
        /// <summary>
        /// Retrieves single feed for passed multiple uris.
        /// </summary>
        Task<IOrderedEnumerable<ArticleEntity>> RetrieveFeedsAsync(IEnumerable<SourceEntity> sourceEntities);
    }
}