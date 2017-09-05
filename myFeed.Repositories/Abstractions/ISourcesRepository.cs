using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Entities.Local;

namespace myFeed.Repositories.Abstractions {
    /// <summary>
    /// Represents sources repository.
    /// </summary>
    public interface ISourcesRepository : IAbstractRepository<SourceCategoryEntity> {
        /// <summary>
        /// Returns all elements ordered by Order field.
        /// </summary>
        Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllOrderedAsync();

        /// <summary>
        /// Renames specified category.
        /// </summary>
        /// <param name="category">Category to rename.</param>
        /// <param name="name">Desired name.</param>
        Task RenameCategoryAsync(SourceCategoryEntity category, string name);

        /// <summary>
        /// Rearranges categories by changing their Order indexes.
        /// </summary>
        /// <param name="categories">Categories order.</param>
        Task RearrangeCategoriesAsync(IEnumerable<SourceCategoryEntity> categories);

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
