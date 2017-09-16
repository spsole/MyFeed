using myFeed.ViewModels.Extensions;

namespace myFeed.Views.Uwp.Platform
{
    public sealed class UwpLocator : Locator<
        UwpTranslationsService,
        UwpPlatformService,
        UwpDialogService,
        UwpFilePickerService> {}
}
