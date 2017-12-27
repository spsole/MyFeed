using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Platform;
using myFeed.Uwp.Services;
using myFeed.ViewModels;

namespace myFeed.Uwp
{
    public sealed partial class App : Application
    {
        public App() => InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureDefaultViewIsPresent();
            if (Guid.TryParse(e.TileId, out var guid))
                OpenArticleViewForPinnedArticleUsingGuid(guid);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            EnsureDefaultViewIsPresent();
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid)) 
                OpenArticleViewForPinnedArticleUsingGuid(guid);
        }

        private static async void EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) await Services.Uwp.Current
                .Resolve<INavigationService>()
                .Navigate<MenuViewModel>();
            Window.Current.Activate();

            var legacyService = Services.Uwp.Current.Resolve<UwpLegacyFileService>();
            await legacyService.ImportFeedsFromLegacyFormat();
            await legacyService.ImportArticlesFromLegacyFormat();
        }

        private static async void OpenArticleViewForPinnedArticleUsingGuid(Guid id) => await Services.Uwp.Current
            .Resolve<INavigationService>()
            .Navigate<ArticleViewModel>(id);
    }
}