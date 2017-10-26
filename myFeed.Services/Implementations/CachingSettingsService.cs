using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class CachingSettingsService : ISettingsService
    {
        private readonly IReadOnlyDictionary<string, string> _defaultConfiguration;
        private readonly IDictionary<string, string> _cachedConfiguration;
        private readonly ISettingsRepository _settingsRepository;
        private readonly SemaphoreSlim _semaphoreSlim;
        
        public CachingSettingsService(
            ISettingsRepository settingsRepository,
            IDefaultsService defaultsService)
        {
            _settingsRepository = settingsRepository;
            _cachedConfiguration = new Dictionary<string, string>();
            _defaultConfiguration = defaultsService.DefaultSettings;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }
        
        public async Task<T> GetAsync<T>(string key) where T : IConvertible
        {
            await _semaphoreSlim.WaitAsync();
            if (_cachedConfiguration.TryGetValue(key, out var cachedValue)) 
                return ReleaseAndRetype(cachedValue);
            
            var storedSetting = await _settingsRepository.GetByKeyAsync(key);
            if (storedSetting?.Value != null) return ReleaseAndRetype(
                _cachedConfiguration[key] = storedSetting.Value);
            
            if (!_defaultConfiguration.TryGetValue(key, out var value))
                throw new ArgumentOutOfRangeException($"No defaults: {key}");
            
            _cachedConfiguration[key] = value;
            await _settingsRepository.InsertAsync(new Setting {Key = key, Value = value});
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
            var existingSetting = await _settingsRepository.GetByKeyAsync(key);
            var converted = value.ToString(CultureInfo.InvariantCulture);
            _cachedConfiguration[key] = converted;
            if (existingSetting == null)
            {
                await _settingsRepository.InsertAsync(new Setting {
                    Key = key, Value = converted });
                return;
            }

            existingSetting.Value = converted;
            await _settingsRepository.UpdateAsync(existingSetting);
        }
    }
}