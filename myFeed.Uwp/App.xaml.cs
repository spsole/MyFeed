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
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI;
using myFeed.Uwp.Views;

namespace myFeed.Uwp
{
    public sealed partial class App : Application
    {
        private readonly IContainer _container = new Container();

        public App() => InitializeComponent();

        private void EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) 
                Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) 
            {
                // Configure database and services.
                var localFolder = ApplicationData.Current.LocalFolder;
                var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
                _container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
                _container.RegisterExports(new[] {typeof(App).GetAssembly()});
                _container.RegisterShared();

                // Initialize root View and ViewModel.
                frame.Navigate(typeof(MenuView));
                var page = (Page)frame.Content;
                page.DataContext = _container.Resolve<MenuViewModel>();
                
                // StatusBar should have the same color as menu page does.
                var color = (Color)Current.Resources["SystemChromeLowColor"];
                StatusBar.SetBackgroundColor(page, color);
                StatusBar.SetBackgroundOpacity(page, 1);
            }
            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureDefaultViewIsPresent();
            if (Guid.TryParse(e.TileId, out var guid))
                await NavigateToToast(guid);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            EnsureDefaultViewIsPresent();
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid))
                await NavigateToToast(guid);
        }

        private async Task NavigateToToast(Guid guid)
        {
            var categoryManager = _container.Resolve<ICategoryManager>();
            var article = await categoryManager.GetArticleById(guid);
            if (article == null) return;

            var factory = _container.Resolve<Func<Article, FeedItemViewModel>>();
            var articleFactory = _container.Resolve<Func<FeedItemViewModel, FeedItemFullViewModel>>();
            var navigationService = _container.Resolve<INavigationService>();
            var articleViewModel = articleFactory(factory(article));

            if (navigationService.CurrentViewModelType != typeof(FeedViewModel))
            {
                await navigationService.Navigate<FeedViewModel>();
                await Task.Delay(150);
            }
            await navigationService.NavigateTo(articleViewModel);
        }
    }
}