using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace myFeed.Extensions.UserControls
{
    public sealed partial class LoadingUserControl
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(LoadingUserControl),
                new PropertyMetadata(true, IsActivePropertyChanged)
            );

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private static async void IsActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var thisControl = (LoadingUserControl)sender;
            var isActive = (bool)args.NewValue;
            if (isActive)
            {
                thisControl.BackGrid.Visibility = Visibility.Visible;
                thisControl.LoadRing.IsActive = true;
                await thisControl.BackGrid.Fade(1, 300).StartAsync();
            }
            else
            {
                thisControl.LoadRing.IsActive = false;
                await thisControl.BackGrid.Fade(0, 300).StartAsync();
                thisControl.BackGrid.Visibility = Visibility.Collapsed;
            }
        }

        public LoadingUserControl()
        {
            InitializeComponent();

            BackGrid.Visibility = Visibility.Visible;
            LoadRing.IsActive = true;
            BackGrid.Fade(value: 1, duration: 300).StartAsync();
        }
    }
}
