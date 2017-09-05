using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    /// <summary>
    /// Represents sources repository.
    /// </summary>
    public class SourcesRepository : AbstractRepository<SourceCategoryEntity>, ISourcesRepository {
        private readonly DbContext _context;

        /// <summary>
        /// Initializes new Sources repository.
        /// </summary>
        public SourcesRepository(EntityContext context) : base(context) => _context = context;

        /// <summary>
        /// Returns all source category entities including table values with Sources.
        /// </summary>
        public override Task<IEnumerable<SourceCategoryEntity>> GetAllAsync() {
            var queryable = GetAllQueryable();
            var enumerable = queryable
                .Include(i => i.Sources)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(enumerable);
        }

        /// <summary>
        /// Returns all entities ordered by Order field.
        /// </summary>
        public Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllOrderedAsync() {
            var queryable = GetAllQueryable();
            var enumerable = queryable
                .Include(i => i.Sources)
                .ToList()
                .OrderBy(i => i.Order);
            return Task.FromResult(enumerable);
        }

        /// <summary>
        /// Inserts entity into DbSet and increments it's order.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        public override async Task InsertAsync(SourceCategoryEntity entity) {
            var queryable = GetAllQueryable();
            var maxOrder = queryable.Max(i => i.Order);
            entity.Order = ++maxOrder;
            await base.InsertAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates order of categories in passed sequence.
        /// </summary>
        /// <param name="categories">Ordered categories.</param>
        public async Task RearrangeCategoriesAsync(IEnumerable<SourceCategoryEntity> categories) {
            var index = 0;
            foreach (var category in categories) category.Order = index++;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Renames category entity.
        /// </summary>
        /// <param name="category">Category to rename.</param>
        /// <param name="name">New name.</param>
        public async Task RenameCategoryAsync(SourceCategoryEntity category, string name) {
            category.Title = name;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds source to category.
        /// </summary>
        /// <param name="category">Category entity.</param>
        /// <param name="source">Source entity.</param>
        public async Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source) {
            category.Sources.Add(source);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes source from category.
        /// </summary>
        /// <param name="category">Category entity.</param>
        /// <param name="source">Source entity.</param>
        public async Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source) {
            category.Sources.Remove(source);
            await _context.SaveChangesAsync();
        }
    }
}