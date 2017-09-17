using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
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
                return entity?.Value;
            }
        }

        public async Task SetByNameAsync(string name, string value)
        {
            using (var context = new EntityContext())
            {
                var set = context.Set<ConfigurationEntity>();
                var entity = await set.FirstOrDefaultAsync(i => i.Key == name);
                if (entity != null) entity.Value = value;
                else set.Add(new ConfigurationEntity {Key = name, Value = value});
                await context.SaveChangesAsync();
            }
        }
    }
}