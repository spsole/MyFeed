using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DryIoc.MefAttributedModel;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using myFeed.ViewModels;
using LiteDB;
using DryIoc;

namespace myFeed.Uwp
{
    public sealed partial class App : Application
    {
        private readonly IContainer _container;

        public App()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            _container = new Container();
            _container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            _container.RegisterExports(new[] {typeof(App).GetAssembly()});
            _container.RegisterShared();
            InitializeComponent();
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

        private async Task EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) await _container.Resolve<INavigationService>().Navigate<MenuViewModel>();
            Window.Current.Activate();
        }

        private async Task NavigateToToast(Guid guid)
        {
            var categoryManager = _container.Resolve<ICategoryManager>();
            var navigationService = _container.Resolve<INavigationService>();
            var article = await categoryManager.GetArticleByIdAsync(guid);
            if (article == null) return;

            var factory = _container.Resolve<Func<Article, FeedItemViewModel>>();
            var articleFactory = _container.Resolve<Func<FeedItemViewModel, ArticleViewModel>>();
            var articleViewModel = articleFactory.Invoke(factory.Invoke(article));
            await navigationService.Navigate<ArticleViewModel>(articleViewModel);
        }
    }
}