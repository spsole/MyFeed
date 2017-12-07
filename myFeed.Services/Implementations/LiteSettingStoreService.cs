using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DryIocAttributes;
using LiteDB;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ISettingStoreService))]
    public sealed class LiteSettingStoreService : ISettingStoreService
    {
        private readonly LiteDatabase _liteDatabase;

        public LiteSettingStoreService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<Setting> GetByKeyAsync(string key) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Setting>();
            return collection.FindOne(i => i.Key == key);
        });

        public Task InsertAsync(Setting setting) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Setting>();
            collection.Insert(setting);
        });

        public Task UpdateAsync(Setting setting) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Setting>();
            collection.Update(setting);
        });
    }
}