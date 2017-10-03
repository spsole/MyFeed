using System;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Responsible for settings managing and caching.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Returns setting value.
        /// </summary>
        Task<TValue> Get<TValue>(string key) where TValue : IConvertible;

        /// <summary>
        /// Updates setting value.
        /// </summary>
        Task Set<TValue>(string key, TValue value) where TValue : IConvertible;
    }
}