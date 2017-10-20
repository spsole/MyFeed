using System;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    public interface ISettingsService
    {
        Task SetAsync<TValue>(string key, TValue value) where TValue : IConvertible;
        
        Task<TValue> GetAsync<TValue>(string key) where TValue : IConvertible;
    }
}