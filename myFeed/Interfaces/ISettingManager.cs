using System;
using System.Threading.Tasks;

namespace myFeed.Interfaces
{
    public interface ISettingManager
    {
        Task SetAsync<TValue>(string key, TValue value) where TValue : IConvertible;
        
        Task<TValue> GetAsync<TValue>(string key) where TValue : IConvertible;
    }
}