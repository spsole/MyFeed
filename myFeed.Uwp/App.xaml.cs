using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DryIoc;
using DryIoc.MefAttributedModel;
using LiteDB;
using myFeed.Interfaces;
using myFeed.Models;
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
            //UnhandledException += (o, shit) => Debug.WriteLine(shit.Exception);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await EnsureDefaultViewIsPresent();
            if (Guid.TryParse(e.TileId, out var guid))
                await NavigateToToast(guid);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await EnsureDefaultViewIsPresent();
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid))
                await NavigateToToast(guid);
        }

        private static async Task EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) await Container.Resolve<INavigationService>().Navigate<MenuViewModel>();
            Window.Current.Activate();

            var legacyService = Container.Resolve<UwpLegacyFileService>();
            await legacyService.ImportFeedsFromLegacyFormat();
            await legacyService.ImportArticlesFromLegacyFormat();
        }

        private static async Task NavigateToToast(Guid guid)
        {
            var factoryService = Container.Resolve<IFactoryService>();
            var categoryManager = Container.Resolve<ICategoryManager>();
            var navigationService = Container.Resolve<INavigationService>();
            var article = await categoryManager.GetArticleByIdAsync(guid);
            if (article == null) return;

            var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
            var articleFactory = factoryService.Create<Func<FeedItemViewModel, ArticleViewModel>>();
            var articleViewModel = articleFactory.Invoke(factory.Invoke(article));
            await navigationService.Navigate<ArticleViewModel>(articleViewModel);
        }
    }
}