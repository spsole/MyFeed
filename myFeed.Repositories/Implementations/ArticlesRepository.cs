using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities;
using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    public class ArticlesRepository : AbstractRepository<ArticleEntity>, IArticlesRepository {
        public ArticlesRepository(EntityContext context) : base(context) {}

        public override Task<IEnumerable<ArticleEntity>> GetAllAsync() {
            var enumerable = GetAllQueryable()
                .Include(i => i.Source)
                .ThenInclude(i => i.Category)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(enumerable);
        }
    }
}
