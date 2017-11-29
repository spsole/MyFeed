using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class LiteDbFavoriteStoreService : IFavoriteStoreService
    {
        private readonly LiteDatabase _liteDatabase;
        
        public LiteDbFavoriteStoreService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<IEnumerable<Article>> GetAllAsync() => Task.Run(() => _liteDatabase
            .GetCollection<Article>().FindAll());

        public Task InsertAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>().Insert(article));

        public Task RemoveAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>().Delete(i => i.Id == article.Id));

        public Task UpdateAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>().Update(article));
    }
}