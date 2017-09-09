using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Entities.Local;

namespace myFeed.Repositories.Abstractions
{
    /// <summary>
    /// Represents sources repository.
    /// </summary>
    public interface ISourcesRepository
    {
        /// <summary>
        /// Returns all elements ordered by Order field.
        /// </summary>
        Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllAsync();

        /// <summary>
        /// Inserts item into database table.
        /// </summary>
        /// <param name="entities">Entity to insert.</param>
        Task InsertAsync(params SourceCategoryEntity[] entities);

        /// <summary>
        /// Removes category from db set.
        /// </summary>
        /// <param name="entities">Entity to remove.</param>
        Task RemoveAsync(params SourceCategoryEntity[] entities);
        
        /// <summary>
        /// Updates category entity.
        /// </summary>
        /// <param name="entity">Category entity to update.</param>
        Task UpdateAsync(SourceCategoryEntity entity);
        
        /// <summary>
        /// Renames specified category.
        /// </summary>
        /// <param name="category">Category to rename.</param>
        /// <param name="name">Desired name.</param>
        Task RenameAsync(SourceCategoryEntity category, string name);

        /// <summary>
        /// Rearranges categories by changing their Order indexes.
        /// </summary>
        /// <param name="categories">Categories order.</param>
        Task RearrangeAsync(IEnumerable<SourceCategoryEntity> categories);

        /// <summary>
        /// Adds source entity to specified category.
        /// </summary>
        /// <param name="category">Category to insert into.</param>
        /// <param name="source">Source to insert.</param>
        Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source);

        /// <summary>
        /// Removes source entity from specified category.
        /// </summary>
        /// <param name="category">Category entity.</param>
        /// <param name="source">Source entity.</param>
        Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source);
    }
}