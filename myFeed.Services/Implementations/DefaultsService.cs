using System.Collections.Generic;
using myFeed.Services.Abstractions;
using System.Globalization;
using System;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IDefaultsService))]
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