using System.ComponentModel.Composition;
using Windows.ApplicationModel.Resources;
using DryIocAttributes;
using myFeed.Services.Platform;

namespace myFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ITranslationsService))]
    public sealed class UwpTranslationsService : ITranslationsService
    {
        public string Resolve(string name) => ResourceLoader.GetForViewIndependentUse().GetString(name);
    }
}