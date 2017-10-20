using System.Collections.Generic;

namespace myFeed.Services.Platform
{
    public interface IDefaultsService
    {
        IReadOnlyDictionary<string, string> DefaultSettings { get; }
    }
}
