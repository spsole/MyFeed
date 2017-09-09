using System;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        public ConfigurationRepository()
        {
            using (var context = new EntityContext())
                if (!context.Database.GetAppliedMigrations().Any())
                    context.Database.Migrate();
        }
        
        public async Task<string> GetByNameAsync(string name)
        {
            using (var context = new EntityContext())
            {
                var entity = await context
                    .Set<ConfigurationEntity>()
                    .FirstOrDefaultAsync(i => i.Key == name);
                return entity.Value;
            }
        }

        public async Task SetByNameAsync(string name, string value)
        {
            using (var context = new EntityContext())
            {
                var entity = await context
                    .Set<ConfigurationEntity>()
                    .FirstOrDefaultAsync(i => i.Key == name);
                if (entity != null)
                {
                    entity.Value = value;
                    await context.SaveChangesAsync();
                    return;
                }
                entity = new ConfigurationEntity { Key = name, Value = value };
                context.Set<ConfigurationEntity>().Add(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}