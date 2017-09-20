using Windows.UI.Xaml;
using myFeed.ViewModels.Extensions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpLocator : Locator<
        UwpTranslationsService,
        UwpPlatformService,
        UwpDialogService,
        UwpFilePickerService, 
        UwpNavigationService>
    {
        public static UwpLocator Current => (UwpLocator)Application.Current.Resources["Locator"];
    }
}
