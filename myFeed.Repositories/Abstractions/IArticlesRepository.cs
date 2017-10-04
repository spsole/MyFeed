using System;
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
        /// Returns article with given id froim database.
        /// </summary>
        Task<ArticleEntity> GetByIdAsync(Guid guid);

        /// <summary>
        /// Returns all articles with all referenced tables.
        /// </summary>
        Task<IEnumerable<ArticleEntity>> GetAllAsync();
        
        /// <summary>
        /// Inserts item into database table.
        /// </summary>
        Task InsertAsync(SourceEntity source, params ArticleEntity[] entities);

        /// <summary>
        /// Removes category from db set.
        /// </summary>
        Task RemoveAsync(params ArticleEntity[] entities);

        /// <summary>
        /// Updates article entity.
        /// </summary>
        Task UpdateAsync(ArticleEntity entity);
        
        /// <summary>
        /// Removes all articles satisfying predicate.
        /// </summary>
        Task RemoveUnreferencedArticles();
    }
}