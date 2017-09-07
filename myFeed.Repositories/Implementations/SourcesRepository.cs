using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    public class SourcesRepository : AbstractRepository<SourceCategoryEntity>, ISourcesRepository {
        private readonly DbContext _context;
        public SourcesRepository(EntityContext context) : base(context) => _context = context;

        public override Task<IEnumerable<SourceCategoryEntity>> GetAllAsync() {
            var queryable = GetAllQueryable();
            var enumerable = queryable
                .Include(i => i.Sources)
                .ThenInclude(i => i.Articles)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(enumerable);
        }

        public Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllOrderedAsync() {
            var queryable = GetAllQueryable();
            var enumerable = queryable
                .Include(i => i.Sources)
                .ThenInclude(i => i.Articles)
                .ToList()
                .OrderBy(i => i.Order);
            return Task.FromResult(enumerable);
        }

        public override async Task InsertAsync(SourceCategoryEntity entity) {
            var queryable = GetAllQueryable();
            var maxOrder = queryable.Max(i => i.Order);
            entity.Order = ++maxOrder;
            await base.InsertAsync(entity).ConfigureAwait(false);
        }

        public async Task RearrangeCategoriesAsync(IEnumerable<SourceCategoryEntity> categories) {
            var index = 0;
            foreach (var category in categories) category.Order = index++;
            await _context.SaveChangesAsync();
        }

        public async Task RenameCategoryAsync(SourceCategoryEntity category, string name) {
            category.Title = name;
            await _context.SaveChangesAsync();
        }

        public async Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source) {
            category.Sources.Add(source);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source) {
            category.Sources.Remove(source);
            await _context.SaveChangesAsync();
        }
    }
}