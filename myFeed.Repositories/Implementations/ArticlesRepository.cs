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
        
        public Task<IEnumerable<ArticleEntity>> GetAllAsync()
        {
            using (var context = new EntityContext())
            {
                var enumerable = context
                    .Set<ArticleEntity>()
                    .Include(i => i.Source)
                    .ThenInclude(i => i.Category)
                    .AsNoTracking()
                    .ToList()
                    .AsEnumerable();
                return Task.FromResult(enumerable);
            }
        }

        public async Task<ArticleEntity> GetByIdAsync(Guid guid)
        {
            using (var context = new EntityContext())
            {
                var article = await context
                    .Set<ArticleEntity>()
                    .FindAsync(guid);
                return article;
            }
        }

        public async Task UpdateAsync(ArticleEntity entity)
        {
            using (var context = new EntityContext())
            {
                context.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }
        
        public async Task RemoveAsync(params ArticleEntity[] entities)
        {
            using (var context = new EntityContext())
            {
                context.AttachRange(entities.AsEnumerable());
                context.Set<ArticleEntity>().RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task InsertAsync(SourceEntity source, params ArticleEntity[] entities)
        {
            using (var context = new EntityContext())
            {
                context.Attach(source);
                context.Attach(source.Category);
                foreach (var entity in entities)
                    source.Articles.Add(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}