using System;
using System.Threading;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using LiteDB;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(ISettingManager))]
    public sealed class LiteSettingManager : ISettingManager
    {
        private readonly Settings _defaultConfiguration;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly LiteDatabase _liteDatabase;
        private Settings _cachedConfiguration;
        
        public LiteSettingManager(LiteDatabase liteDatabase)
        {
            _cachedConfiguration = null;
            _liteDatabase = liteDatabase;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _defaultConfiguration = new Settings
            {
                Banners = false,
                Fetched = DateTime.Now,
                Theme = "default",
                Images = true,
                Read = true,
                Period = 60,
                Max = 100,
                Font = 17
            };
        }

        public async Task<bool> Write(Settings settings)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            var collection = _liteDatabase.GetCollection<Settings>();
            var existing = collection.FindOne(x => true);

            if (existing == null) collection.Insert(settings);
            else collection.Update(Copy(settings, existing));
            
            _cachedConfiguration = new Settings();
            Copy(settings, _cachedConfiguration);
            _semaphoreSlim.Release();
            return true;
        }

        public async Task<Settings> Read()
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            if (_cachedConfiguration != null) return ReleaseReturn(_cachedConfiguration);
            
            var collection = _liteDatabase.GetCollection<Settings>();
            var settings = collection.FindOne(x => true);
            if (settings != null) return ReleaseReturn(_cachedConfiguration = settings);

            collection.Insert(_defaultConfiguration);
            return ReleaseReturn(_defaultConfiguration);
        }

        private Settings ReleaseReturn(Settings from)
        {
            _semaphoreSlim.Release();
            return Copy(from, new Settings());
        }

        private static Settings Copy(Settings from, Settings to)
        {
            to.Theme = from.Theme;
            to.Banners = from.Banners;
            to.Fetched = from.Fetched;
            to.Period = from.Period;
            to.Images = from.Images;
            to.Read = from.Read;
            to.Font = from.Font;
            to.Max = from.Max;
            return to;
        }
    }
}