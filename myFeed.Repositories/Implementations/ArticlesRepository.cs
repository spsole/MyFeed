using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations
{
    public class ArticlesRepository : IArticlesRepository
    {
        public ArticlesRepository()
        {
            using (var context = new EntityContext())
                if (!context.Database.GetAppliedMigrations().Any())
                    context.Database.Migrate();
        }

        public Task<IEnumerable<ArticleEntity>> GetAllAsync() => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                return context
                    .Set<ArticleEntity>()
                    .Include(i => i.Source)
                    .ThenInclude(i => i.Category)
                    .AsNoTracking()
                    .ToList()
                    .AsEnumerable();
            }
        });

        public Task<ArticleEntity> GetByIdAsync(Guid guid) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                return context.Set<ArticleEntity>().FindAsync(guid);
            }
        });

        public Task UpdateAsync(ArticleEntity entity) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
                return context.SaveChangesAsync();
            }
        });

        public Task RemoveAsync(params ArticleEntity[] entities) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.AttachRange(entities.AsEnumerable());
                context.Set<ArticleEntity>().RemoveRange(entities);
                return context.SaveChangesAsync();
            }
        });

        public Task InsertAsync(SourceEntity source, params ArticleEntity[] entities) => Task.Run(() =>
        {
            using (var context = new EntityContext())
            {
                context.Attach(source);
                context.Attach(source.Category);
                foreach (var entity in entities) source.Articles.Add(entity);
                return context.SaveChangesAsync();
            }
        });
    }
}