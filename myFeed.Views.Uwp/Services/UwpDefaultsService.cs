using System.Collections.Generic;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpDefaultsService : IDefaultsService
    {
        public IReadOnlyDictionary<string, string> DefaultSettings => new Dictionary<string, string>
        {
            {"NeedBanners", "true"},
            {"LoadImages", "true"},
            {"NotifyPeriod", "14"},
            {"Theme", "default"},
            {"FontSize", "17"}
        };
    }
}
