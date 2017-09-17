using myFeed.ViewModels.Extensions;

namespace myFeed.Views.Uwp.Platform
{
    public sealed class UwpLocator : ViewModelLocator<
        UwpTranslationsService,
        UwpPlatformService,
        UwpDialogService,
        UwpFilePickerService> {}
}
