using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Feed.Controls
{
    public sealed partial class WelcomeControl
    {
        public WelcomeControl() => InitializeComponent();

        /// <summary>
        /// Invoked when refresh is requested by the user.
        /// </summary>
        public event RoutedEventHandler NavRequested
        {
            add => RefreshButton.Click += value;
            remove => RefreshButton.Click -= value;
        }
    }
}
