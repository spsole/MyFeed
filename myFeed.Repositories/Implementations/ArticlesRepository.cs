using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    /// <summary>
    /// Articles repository to work with articles.
    /// </summary>
    public class ArticlesRepository : AbstractRepository<ArticleEntity>, IArticlesRepository {
        /// <summary>
        /// Initializes new Articles repository.
        /// </summary>
        public ArticlesRepository(EntityContext context) : base(context) { }

        /// <summary>
        /// Returns all items enumerable joined with entities from sources table.
        /// </summary>
        public override Task<IEnumerable<ArticleEntity>> GetAllAsync() {
            var enumerable = GetAllQueryable()
                .Include(i => i.Source)
                .AsEnumerable();
            return Task.FromResult(enumerable);
        }
    }
}
