using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    /// <summary>
    /// Configuration repository.
    /// </summary>
    public class ConfigurationRepository : AbstractRepository<ConfigurationEntity>, IConfigurationRepository {
        private readonly DbContext _context;

        /// <summary>
        /// Initializes new configuration repository.
        /// </summary>
        public ConfigurationRepository(EntityContext context) : base(context) => _context = context;

        /// <summary>
        /// Gets entity using its key.
        /// </summary>
        /// <param name="name">Key to search for.</param>
        public async Task<string> GetByNameAsync(string name) {
            var queryable = GetAllQueryable();
            var entity = await queryable.FirstOrDefaultAsync(i => i.Key == name);
            return entity.Value;
        }

        /// <summary>
        /// Gets entity by it's key and updates it's value. If entity does not
        /// exist, creates new one with specified key and value.
        /// </summary>
        /// <param name="name">Key to search for.</param>
        /// <param name="value">Value to set.</param>
        public async Task SetByNameAsync(string name, string value) {
            var queryable = GetAllQueryable();
            var entity = await queryable.FirstOrDefaultAsync(i => i.Key == name);
            if (entity != null) {
                entity.Value = value;
                await _context.SaveChangesAsync();
                return;
            }
            await InsertAsync(new ConfigurationEntity() {
                Key = name,
                Value = value
            });
        }
    }
}
