using System.Collections.Generic;

namespace myFeed.Interfaces
{
    public interface IDefaultsService
    {
        IReadOnlyDictionary<string, string> DefaultSettings { get; }
    }
}
