using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;

namespace myFeed.Repositories.Implementations
{
    public sealed class FavoritesRepository : IFavoritesRepository
    {
        private readonly LiteDatabase _liteDatabase;
        
        public FavoritesRepository(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<IEnumerable<Article>> GetAllAsync() => Task.Run(() => _liteDatabase
            .GetCollection<Article>()
            .FindAll());

        public Task InsertAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>()
            .Insert(article));

        public Task RemoveAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>()
            .Delete(i => i.Id == article.Id));

        public Task UpdateAsync(Article article) => Task.Run(() => _liteDatabase
            .GetCollection<Article>()
            .Update(article));
    }
}