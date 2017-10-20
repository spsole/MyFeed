using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;

namespace myFeed.Services.Implementations
{
    public sealed class CachingSettingsService : ISettingsService
    {
        private readonly IReadOnlyDictionary<string, string> _defaultConfiguration;
        private readonly IDictionary<string, string> _cachedConfiguration;
        private readonly ISettingsRepository _settingsRepository;
        
        public CachingSettingsService(
            ISettingsRepository settingsRepository,
            IDefaultsService defaultsService)
        {
            _settingsRepository = settingsRepository;
            _cachedConfiguration = new Dictionary<string, string>();
            _defaultConfiguration = defaultsService.DefaultSettings;
        }
        
        public async Task<TValue> GetAsync<TValue>(string key) where TValue : IConvertible
        {
            if (_cachedConfiguration.TryGetValue(key, out var cachedValue)) 
                return SwitchType<TValue>(cachedValue);
            
            var storedSetting = await _settingsRepository.GetByKeyAsync(key);
            if (storedSetting?.Value == null) 
            {
                if (!_defaultConfiguration.TryGetValue(key, out var value))
                    throw new ArgumentOutOfRangeException(
                        "No default value found: " + key);
                
                _cachedConfiguration[key] = value;
                await _settingsRepository.InsertAsync(new Setting 
                {
                    Key = key, Value = value
                });
                return SwitchType<TValue>(value);
            }
            
            _cachedConfiguration[key] = storedSetting.Value;
            return SwitchType<TValue>(storedSetting.Value);
        }

        public async Task SetAsync<TValue>(string key, TValue value) where TValue : IConvertible
        {
            var existingSetting = await _settingsRepository.GetByKeyAsync(key);
            var stringifiedValue = value.ToString(CultureInfo.InvariantCulture);
            _cachedConfiguration[key] = stringifiedValue;
            
            if (existingSetting == null)
            {
                await _settingsRepository.InsertAsync(new Setting
                {
                    Key = key, Value = stringifiedValue
                });
            }
            else
            {
                existingSetting.Value = stringifiedValue;
                await _settingsRepository.UpdateAsync(existingSetting);
            }
        }
        
        private static TValue SwitchType<TValue>(string value) where TValue : IConvertible
        {
            return (TValue) Convert.ChangeType(value, typeof(TValue), CultureInfo.InvariantCulture);
        }
    }
}