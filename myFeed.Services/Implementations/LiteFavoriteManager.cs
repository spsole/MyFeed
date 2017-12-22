using System.Collections.Generic;
using System.ComponentModel.Composition;
using myFeed.Services.Abstractions;
using System.Threading.Tasks;
using myFeed.Services.Models;
using DryIocAttributes;
using LiteDB;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFavoriteManager))]
    public sealed class LiteFavoriteManager : IFavoriteManager
    {
        private readonly ICategoryManager _categoriesRepository;
        private readonly LiteDatabase _liteDatabase;

        public LiteFavoriteManager(
            ICategoryManager categoriesRepository,
            LiteDatabase liteDatabase)
        {
            _categoriesRepository = categoriesRepository;
            _liteDatabase = liteDatabase;
        }

        public Task<IEnumerable<Article>> GetAllAsync() => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Article>();
            return collection.FindAll();
        });

        public Task InsertAsync(Article article) => Task.Run(() =>
        {
            if (article.Fave) return Task.CompletedTask;
            article.Fave = true;
            _liteDatabase.GetCollection<Article>().Insert(article);
            return _categoriesRepository.UpdateArticleAsync(article);
        });

        public Task RemoveAsync(Article article) => Task.Run(() =>
        {
            if (!article.Fave) return Task.CompletedTask;
            article.Fave = false;
            _liteDatabase.GetCollection<Article>().Delete(i => i.Id == article.Id);
            return _categoriesRepository.UpdateArticleAsync(article);
        });
    }
}
