using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public class SettingsService : ISettingsService
    {
        private readonly IReadOnlyDictionary<string, string> _defaultConfiguration;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IDictionary<string, string> _cachedConfiguration;
        
        public SettingsService(
            IDefaultsService defaultsService,
            IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _cachedConfiguration = new Dictionary<string, string>();
            _defaultConfiguration = defaultsService.DefaultSettings;
        }
        
        public async Task<TValue> Get<TValue>(string key) where TValue : IConvertible
        {
            if (_cachedConfiguration.TryGetValue(key, out var cachedValue)) 
                return SwitchType<TValue>(cachedValue);
            
            var storedValue = await _configurationRepository.GetByNameAsync(key);
            var value = storedValue ?? _defaultConfiguration[key];
            if (storedValue == null) await Set(key, value);
            
            _cachedConfiguration[key] = value;
            return SwitchType<TValue>(value);
        }

        public async Task Set<TValue>(string key, TValue value) where TValue : IConvertible
        {
            var stringifiedValue = value.ToString(CultureInfo.InvariantCulture);
            await _configurationRepository.SetByNameAsync(key, stringifiedValue);
            _cachedConfiguration[key] = stringifiedValue;
        }
        
        private static TValue SwitchType<TValue>(string value) where TValue : IConvertible
        {
            return (TValue) Convert.ChangeType(value, typeof(TValue));
        }
    }
}