using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Entities.Local;

namespace myFeed.Repositories.Abstractions
{
    /// <summary>
    /// Articles repository to work with articles.
    /// </summary>
    public interface IArticlesRepository
    {
        /// <summary>
        /// Returns all articles with all referenced tables.
        /// </summary>
        Task<IEnumerable<ArticleEntity>> GetAllAsync();
        
        /// <summary>
        /// Inserts item into database table.
        /// </summary>
        /// <param name="entities">Entity to insert.</param>
        /// <param name="source">Source to attach article entities to.</param>
        Task InsertAsync(SourceEntity source, params ArticleEntity[] entities);

        /// <summary>
        /// Removes category from db set.
        /// </summary>
        /// <param name="entities">Entity to remove.</param>
        Task RemoveAsync(params ArticleEntity[] entities);

        /// <summary>
        /// Updates article entity.
        /// </summary>
        /// <param name="entity">Article entity to update.</param>
        Task UpdateAsync(ArticleEntity entity);
    }
}