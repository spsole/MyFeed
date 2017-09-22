using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Views.Uwp.Views;

namespace myFeed.Views.Uwp
{
    public sealed partial class App : Application
    {
        public App() => InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            UnhandledException += (_, args) => Debug.WriteLine(args.Message);
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            Window.Current.Activate();

            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) frame.Navigate(typeof(MenuView));

            /*
            // Register notifications.
            switch (e.TileId)
            {
                default:
                    await Task.Delay(300);
                    if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(Fave.FavePage))
                        NavigationPage.NavigationFrame.Navigate(typeof(Fave.FavePage));
                    var articles = await Fave.FaveManager.GetInstance().LoadArticles();
                    var target = articles.FirstOrDefault(i => i.GetModel().GetTileId() == e.TileId);
                    if (target != null)
                        Fave.FavePage.NavigationFrame.Navigate(
                            typeof(ArticlePage), target);
                    break;
            }
            */
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            /*
            if (args.Kind != ActivationKind.ToastNotification) return;
            // Build content frame.
            // Find appropriate category.
            // Retrieve data for category.
            // Navigate and mark as read.
            */
        }
    }
}