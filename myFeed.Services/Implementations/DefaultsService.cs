using System.Collections.Generic;
using myFeed.Services.Abstractions;
using System.Globalization;
using System;

namespace myFeed.Services.Implementations
{
    public sealed class DefaultsService : IDefaultsService
    {
        public IReadOnlyDictionary<string, string> DefaultSettings => new Dictionary<string, string>
        {
            {"LastFetched", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
            {"MaxArticlesPerFeed", "100"},
            {"NeedBanners", "true"},
            {"LoadImages", "true"},
            {"NotifyPeriod", "60"},
            {"Theme", "default"},
            {"FontSize", "17"}
        };
    }
}