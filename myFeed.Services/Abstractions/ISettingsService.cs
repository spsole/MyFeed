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
        /// <param name="key">Key to search for.</param>
        Task<TValue> Get<TValue>(string key) where TValue : IConvertible;

        /// <summary>
        /// Updates setting value.
        /// </summary>
        /// <param name="key">Key to update.</param>
        /// <param name="value">Value to set.</param>
        Task Set<TValue>(string key, TValue value) where TValue : IConvertible;
    }
}