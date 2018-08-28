using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MyFeed.Uwp.Views;

namespace MyFeed.Uwp
{
    public sealed partial class App : Application
    {
        public App() => InitializeComponent();

        private void EnsureDefaultViewIsPresent()
        {
            if (Window.Current.Content == null) 
                Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) 
                frame.Navigate(typeof(MenuView));
            Window.Current.Activate();

            var page = (Page)frame.Content;
            var color = (Color)Current.Resources["SystemChromeLowColor"];
            StatusBar.SetBackgroundColor(page, color);
            StatusBar.SetBackgroundOpacity(page, 1);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureDefaultViewIsPresent();
            var bootstrapper = Resources["Bootstrapper"] as Bootstrapper;
            if (Guid.TryParse(e.TileId, out var guid))
                await bootstrapper.NavigateToToast(guid);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            EnsureDefaultViewIsPresent();
            var bootstrapper = Resources["Bootstrapper"] as Bootstrapper;
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid))
                await bootstrapper.NavigateToToast(guid);
        }
    }
}