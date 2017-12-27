using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DryIocAttributes;
using LiteDB;
using myFeed.Interfaces;
using myFeed.Models;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFavoriteManager))]
    public sealed class LiteFavoriteManager : IFavoriteManager
    {
        private readonly ICategoryManager _categoryManager;
        private readonly LiteDatabase _liteDatabase;

        public LiteFavoriteManager(
            ICategoryManager categoryManager,
            LiteDatabase liteDatabase)
        {
            _categoryManager = categoryManager;
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
            return _categoryManager.UpdateArticleAsync(article);
        });

        public Task RemoveAsync(Article article) => Task.Run(() =>
        {
            if (!article.Fave) return Task.CompletedTask;
            article.Fave = false;
            _liteDatabase.GetCollection<Article>().Delete(i => i.Id == article.Id);
            return _categoryManager.UpdateArticleAsync(article);
        });
    }
}
