using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations
{
    public sealed class ConfigurationRepository : IConfigurationRepository
    {
        public ConfigurationRepository()
        {
            using (var context = new EntityContext())
                if (!context.Database.GetAppliedMigrations().Any())
                    context.Database.Migrate();
        }

        public Task<string> GetByNameAsync(string name) => Task.Run(async () =>
        {
            using (var context = new EntityContext())
            {
                var entity = await context
                    .Set<ConfigurationEntity>()
                    .FirstOrDefaultAsync(i => i.Key == name);
                return entity?.Value;
            }
        });

        public Task SetByNameAsync(string name, string value) => Task.Run(async () =>
        {
            using (var context = new EntityContext())
            {
                var set = context.Set<ConfigurationEntity>();
                var entity = await set.FirstOrDefaultAsync(i => i.Key == name);
                if (entity != null) entity.Value = value;
                else set.Add(new ConfigurationEntity {Key = name, Value = value});
                context.SaveChanges();
            }
        });
    }
}