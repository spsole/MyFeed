using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Repositories.Entities.Local;

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
        Task InsertAsync(params ArticleEntity[] entities);

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