using System.Collections.Generic;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Service storing defaults for platform.
    /// </summary>
    public interface IDefaultsService
    {
        /// <summary>
        /// Stores default settings.
        /// </summary>
        IReadOnlyDictionary<string, string> DefaultSettings { get; }
    }
}
