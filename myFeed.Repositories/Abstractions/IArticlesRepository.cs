using myFeed.Repositories.Entities.Local;

namespace myFeed.Repositories.Abstractions {
    /// <summary>
    /// Articles repository to work with articles.
    /// </summary>
    public interface IArticlesRepository : IAbstractRepository<ArticleEntity> { }
}
