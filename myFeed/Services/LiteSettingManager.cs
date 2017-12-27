using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DryIocAttributes;
using LiteDB;
using myFeed.Interfaces;
using myFeed.Models;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ISettingManager))]
    public sealed class LiteSettingManager : ISettingManager
    {
        private readonly IReadOnlyDictionary<string, string> _defaultConfiguration;
        private readonly IDictionary<string, string> _cachedConfiguration;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly LiteDatabase _liteDatabase;
        
        public LiteSettingManager(
            IDefaultsService defaultsService,
            LiteDatabase liteDatabase)
        {
            _defaultConfiguration = defaultsService.DefaultSettings;
            _cachedConfiguration = new Dictionary<string, string>();
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _liteDatabase = liteDatabase;
        }
        
        public async Task<T> GetAsync<T>(string key) where T : IConvertible
        {
            await _semaphoreSlim.WaitAsync();
            if (_cachedConfiguration.TryGetValue(key, out var cachedValue)) 
                return ReleaseAndRetype(cachedValue);

            var collection = _liteDatabase.GetCollection<Setting>();
            var storedSetting = collection.FindOne(i => i.Key == key);
            if (storedSetting?.Value != null) return ReleaseAndRetype(
                _cachedConfiguration[key] = storedSetting.Value);
            
            if (!_defaultConfiguration.TryGetValue(key, out var value))
                throw new ArgumentOutOfRangeException($"No defaults: {key}");
            
            _cachedConfiguration[key] = value;
            collection.Insert(new Setting {Key = key, Value = value});
            return ReleaseAndRetype(value);

            T ReleaseAndRetype(string convertible)
            {
                _semaphoreSlim.Release();
                return (T) Convert.ChangeType(convertible, 
                    typeof(T), CultureInfo.InvariantCulture);
            }
        }

        public async Task SetAsync<T>(string key, T value) where T : IConvertible
        {
            await _semaphoreSlim.WaitAsync();
            var collection = _liteDatabase.GetCollection<Setting>();
            var existingSetting = collection.FindOne(i => i.Key == key);
            var converted = value.ToString(CultureInfo.InvariantCulture);
            
            _cachedConfiguration[key] = converted;
            if (existingSetting == null)
            {
                collection.Insert(new Setting {Key = key, Value = converted});
                _semaphoreSlim.Release();
                return;
            }
            
            existingSetting.Value = converted;
            collection.Update(existingSetting);
            _semaphoreSlim.Release();
        }
    }
}