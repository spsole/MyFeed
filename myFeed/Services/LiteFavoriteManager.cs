using System.Collections.Generic;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using LiteDB;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IFavoriteManager))]
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

        public Task<IEnumerable<Article>> GetAll() => Task.Run(
            () => _liteDatabase.GetCollection<Article>().FindAll()
        );

        public async Task Insert(Article article) 
        {
            if (article.Fave) return;
            article.Fave = true;
            await _categoryManager.Update(article).ConfigureAwait(false);
            _liteDatabase.GetCollection<Article>().Insert(article);
        }

        public async Task Remove(Article article)
        {
            if (!article.Fave) return;
            article.Fave = false;
            await _categoryManager.Update(article).ConfigureAwait(false);
            _liteDatabase.GetCollection<Article>().Delete(i => i.Id == article.Id);
        }
    }
}
