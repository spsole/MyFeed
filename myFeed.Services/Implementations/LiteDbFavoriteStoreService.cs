using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DryIocAttributes;
using LiteDB;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFavoriteStoreService))]
    public sealed class LiteDbFavoriteStoreService : IFavoriteStoreService
    {
        private readonly LiteDatabase _liteDatabase;
        
        public LiteDbFavoriteStoreService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<IEnumerable<Article>> GetAllAsync() => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Article>();
            return collection.FindAll();
        });

        public Task InsertAsync(Article article) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Article>();
            collection.Insert(article);
        });

        public Task RemoveAsync(Article article) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Article>();
            collection.Delete(i => i.Id == article.Id);
        });

        public Task UpdateAsync(Article article) => Task.Run(() => 
        {
            var collection = _liteDatabase.GetCollection<Article>();
            collection.Update(article);
        });
    }
}