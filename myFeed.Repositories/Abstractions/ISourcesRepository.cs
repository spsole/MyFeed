using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities.Local;

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
        Task InsertAsync(params SourceCategoryEntity[] entities);

        /// <summary>
        /// Removes category from db set.
        /// </summary>
        Task RemoveAsync(params SourceCategoryEntity[] entities);
        
        /// <summary>
        /// Updates category entity.
        /// </summary>
        Task UpdateAsync(SourceCategoryEntity entity);
        
        /// <summary>
        /// Renames specified category.
        /// </summary>
        Task RenameAsync(SourceCategoryEntity category, string name);

        /// <summary>
        /// Rearranges categories by changing their Order indexes.
        /// </summary>
        Task RearrangeAsync(IEnumerable<SourceCategoryEntity> categories);

        /// <summary>
        /// Adds source entity to specified category.
        /// </summary>
        Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source);

        /// <summary>
        /// Removes source entity from specified category.
        /// </summary>
        Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source);
    }
}