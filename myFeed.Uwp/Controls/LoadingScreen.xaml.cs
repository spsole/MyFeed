using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;

namespace myFeed.Uwp.Controls
{
    public sealed partial class LoadingScreen
    {
        public LoadingScreen()
        {
            InitializeComponent();
            BackGrid.Visibility = Visibility.Visible;
            LoadRing.IsActive = true;
            BackGrid.Opacity = 1;
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(LoadingScreen), new PropertyMetadata(true, 
                (s, o) =>  IsActivePropertyChanged(s, (bool)o.NewValue)));

        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private static async void IsActivePropertyChanged(DependencyObject sender, bool isActive)
        {
            var thisControl = (LoadingScreen) sender;
            if (isActive && !thisControl.LoadRing.IsActive)
            {
                thisControl.LoadRing.IsActive = true;
                thisControl.BackGrid.Visibility = Visibility.Visible;
                await thisControl.BackGrid.Fade(1, 800).StartAsync();
            }
            else
            {
                await thisControl.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                {
                    thisControl.LoadRing.IsActive = false;
                    await thisControl.BackGrid.Fade(0, 800).StartAsync();
                    thisControl.BackGrid.Visibility = Visibility.Collapsed;
                });
            }
        }
    }
}