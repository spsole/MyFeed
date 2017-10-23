using System.Collections.Generic;

namespace myFeed.Services.Abstractions
{
    public interface IDefaultsService
    {
        IReadOnlyDictionary<string, string> DefaultSettings { get; }
    }
}
