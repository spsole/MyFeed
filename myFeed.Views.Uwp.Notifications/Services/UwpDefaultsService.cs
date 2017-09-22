using System.Collections.Generic;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Notifications.Services
{
    internal sealed class UwpDefaultsService : IDefaultsService
    {
        public IReadOnlyDictionary<string, string> DefaultSettings => new Dictionary<string, string>
        {
            {"NeedBanners", "true"},
            {"LoadImages", "true"},
            {"NotifyPeriod", "14"},
            {"Theme", "default"},
            {"FontSize", "14"}
        };
    }
}
