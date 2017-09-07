using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    public class ConfigurationRepository : AbstractRepository<ConfigurationEntity>, IConfigurationRepository {
        private readonly DbContext _context;
        public ConfigurationRepository(EntityContext context) : base(context) => _context = context;

        public async Task<string> GetByNameAsync(string name) {
            var queryable = GetAllQueryable();
            var entity = await queryable.FirstOrDefaultAsync(i => i.Key == name);
            return entity.Value;
        }

        public async Task SetByNameAsync(string name, string value) {
            var queryable = GetAllQueryable();
            var entity = await queryable.FirstOrDefaultAsync(i => i.Key == name);
            if (entity != null) {
                entity.Value = value;
                await _context.SaveChangesAsync();
            } else {
                await InsertAsync(new ConfigurationEntity { Key = name, Value = value });
            }
        }
    }
}
