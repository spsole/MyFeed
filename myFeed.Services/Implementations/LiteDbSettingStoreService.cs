using System.Threading.Tasks;
using LiteDB;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class LiteDbSettingStoreService : ISettingStoreService
    {
        private readonly LiteDatabase _liteDatabase;
        
        public LiteDbSettingStoreService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<Setting> GetByKeyAsync(string key) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>().FindOne(i => i.Key == key));

        public Task InsertAsync(Setting setting) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>().Insert(setting));

        public Task UpdateAsync(Setting setting) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>().Update(setting));
    }
}