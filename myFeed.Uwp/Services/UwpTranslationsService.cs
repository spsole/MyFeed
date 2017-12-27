using System.ComponentModel.Composition;
using Windows.ApplicationModel.Resources;
using DryIocAttributes;
using myFeed.Platform;

namespace myFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ITranslationService))]
    public sealed class UwpTranslationService : ITranslationService
    {
        public string Resolve(string name) => ResourceLoader.GetForViewIndependentUse().GetString(name);
    }
}