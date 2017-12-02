using Windows.ApplicationModel.Resources;
using myFeed.Services.Platform;
using DryIocAttributes;
using System.ComponentModel.Composition;

namespace myFeed.Views.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ITranslationsService))]
    public sealed class UwpTranslationsService : ITranslationsService
    {
        public string Resolve(string name) => ResourceLoader.GetForViewIndependentUse().GetString(name);
    }
}