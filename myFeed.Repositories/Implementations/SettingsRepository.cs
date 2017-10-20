using System.Threading.Tasks;
using LiteDB;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;

namespace myFeed.Repositories.Implementations
{
    public sealed class SettingsRepository : ISettingsRepository
    {
        private readonly LiteDatabase _liteDatabase;
        
        public SettingsRepository(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<Setting> GetByKeyAsync(string key) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>()
            .FindOne(i => i.Key == key));

        public Task InsertAsync(Setting setting) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>()
            .Insert(setting));

        public Task UpdateAsync(Setting setting) => Task.Run(() => _liteDatabase
            .GetCollection<Setting>()
            .Update(setting));
    }
}