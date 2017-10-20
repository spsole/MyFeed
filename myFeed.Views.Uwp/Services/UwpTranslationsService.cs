using Windows.ApplicationModel.Resources;
using myFeed.Services.Platform;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpTranslationsService : ITranslationsService
    {
        public string Resolve(string name) => ResourceLoader.GetForViewIndependentUse().GetString(name);
    }
}