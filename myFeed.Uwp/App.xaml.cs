using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DryIoc;
using DryIoc.MefAttributedModel;
using LiteDB;
using myFeed.Platform;
using myFeed.Uwp.Services;
using myFeed.ViewModels;

namespace myFeed.Uwp
{
    public sealed partial class App : Application
    {
        public static IContainer Container { get; } = new Container();

        public App()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            Container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            Container.RegisterExports(new[] {typeof(App).GetAssembly()});
            Container.RegisterShared();
            InitializeComponent();

            this.UnhandledException += (o, shit) => Debug.WriteLine(shit.Exception);
        }

        private static async void EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) await Container.Resolve<INavigationService>().Navigate<MenuViewModel>();
            Window.Current.Activate();

            var legacyService = Container.Resolve<UwpLegacyFileService>();
            await legacyService.ImportFeedsFromLegacyFormat();
            await legacyService.ImportArticlesFromLegacyFormat();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureDefaultViewIsPresent();
            if (Guid.TryParse(e.TileId, out var guid))
                Container.Resolve<INavigationService>().Navigate<ArticleViewModel>(guid);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            EnsureDefaultViewIsPresent();
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid))
                Container.Resolve<INavigationService>().Navigate<ArticleViewModel>(guid);
        }
    }
}