using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations
{
    public class SourcesRepository : ISourcesRepository
    {
        public SourcesRepository()
        {
            using (var context = new EntityContext())
                if (!context.Database.GetAppliedMigrations().Any())
                    context.Database.Migrate();
        }
        
        public Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllAsync()
        {
            using (var context = new EntityContext())
            {
                var enumerable = context
                    .Set<SourceCategoryEntity>()
                    .Include(i => i.Sources)
                    .ThenInclude(i => i.Articles)
                    .AsNoTracking()
                    .ToList()
                    .OrderBy(i => i.Order);
                return Task.FromResult(enumerable);
            }
        }

        public async Task InsertAsync(params SourceCategoryEntity[] entities)
        {
            using (var context = new EntityContext())
            {
                var queryable = context.Set<SourceCategoryEntity>();
                var max = queryable.Max(i => i.Order);
                foreach (var entity in entities) 
                    entity.Order = ++max;
                queryable.AddRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(params SourceCategoryEntity[] entities)
        {
            using (var context = new EntityContext())
            {
                context.AttachRange(entities.AsEnumerable());
                context.Set<SourceCategoryEntity>().RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task RearrangeAsync(IEnumerable<SourceCategoryEntity> categories)
        {
            using (var context = new EntityContext())
            {
                var categoriesList = categories.ToList();
                var index = 0;
                context.AttachRange(categoriesList);
                foreach (var category in categoriesList) 
                    category.Order = index++;
                await context.SaveChangesAsync();
            }
        }

        public async Task RenameAsync(SourceCategoryEntity category, string name)
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                category.Title = name;
                await context.SaveChangesAsync();
            }
        }

        public async Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source) 
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                category.Sources.Add(source);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source)
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                context.Attach(source);
                category.Sources.Remove(source);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(SourceCategoryEntity entity)
        {
            using (var context = new EntityContext())
            {
                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }
    }
}