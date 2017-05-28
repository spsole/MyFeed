using Windows.UI.Xaml;

namespace myFeed.Feed.Controls
{
    public sealed partial class StartScreen
    {
        public StartScreen() => InitializeComponent();

        /// <summary>
        /// Invoked when refresh is requested by the user.
        /// </summary>
        public event RoutedEventHandler NavRequested
        {
            add => NavButton.Click += value;
            remove => NavButton.Click -= value;
        }
    }
}
