using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations
{
    public sealed class SourcesRepository : ISourcesRepository
    {
        public SourcesRepository()
        {
            using (var context = new EntityContext())
                if (!context.Database.GetAppliedMigrations().Any())
                    context.Database.Migrate();
        }

        public Task<IOrderedEnumerable<SourceCategoryEntity>> GetAllAsync() => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                return context
                    .Set<SourceCategoryEntity>()
                    .Include(i => i.Sources)
                    .ThenInclude(i => i.Articles)
                    .AsNoTracking()
                    .ToList()
                    .OrderBy(i => i.Order);
            }
        });

        public Task InsertAsync(params SourceCategoryEntity[] entities) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                var queryable = context.Set<SourceCategoryEntity>();
                var max = queryable.Max(i => i.Order);
                foreach (var entity in entities)
                    entity.Order = ++max;
                queryable.AddRange(entities);
                return context.SaveChangesAsync();
            }
        });

        public Task RemoveAsync(params SourceCategoryEntity[] entities) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.AttachRange(entities.AsEnumerable());
                context.Set<SourceCategoryEntity>().RemoveRange(entities);
                return context.SaveChangesAsync();
            }
        });

        public Task RearrangeAsync(IEnumerable<SourceCategoryEntity> categories) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                var categoriesList = categories.ToList();
                var index = 0;
                context.AttachRange(categoriesList);
                foreach (var category in categoriesList)
                    category.Order = index++;
                return context.SaveChangesAsync();
            }
        });

        public Task RenameAsync(SourceCategoryEntity category, string name) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                category.Title = name;
                return context.SaveChangesAsync();
            }
        });

        public Task AddSourceAsync(SourceCategoryEntity category, SourceEntity source) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                category.Sources.Add(source);
                return context.SaveChangesAsync();
            }
        });

        public Task RemoveSourceAsync(SourceCategoryEntity category, SourceEntity source) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Attach(category);
                context.Attach(source);
                category.Sources.Remove(source);
                return context.SaveChangesAsync();
            }
        });

        public Task UpdateAsync(SourceCategoryEntity entity) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Entry(entity).State = EntityState.Modified;
                return context.SaveChangesAsync();
            }
        });
    }
}