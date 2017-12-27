using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using DryIocAttributes;
using myFeed.Interfaces;

namespace myFeed.Services
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