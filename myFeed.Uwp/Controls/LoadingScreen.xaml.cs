using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace myFeed.Uwp.Controls
{
    public sealed partial class LoadingScreen
    {
        public LoadingScreen()
        {
            InitializeComponent();
            IsActivePropertyChanged(this, true);
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
            if (isActive)
            {
                thisControl.BackGrid.Visibility = Visibility.Visible;
                thisControl.LoadRing.IsActive = true;
                await thisControl.BackGrid.Fade(1, 1000).StartAsync();
            }
            else
            {
                thisControl.LoadRing.IsActive = false;
                await thisControl.BackGrid.Fade(0, 1000).StartAsync();
                thisControl.BackGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}