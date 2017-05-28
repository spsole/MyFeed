using Windows.UI.Xaml;

namespace myFeed.Sources.Controls
{
    public sealed partial class WelcomeControl
    {
        public WelcomeControl() => InitializeComponent();

        /// <summary>
        /// Invoked when add action is requested by the user.
        /// </summary>
        public event RoutedEventHandler AddRequested
        {
            add => NavButton.Click += value;
            remove => NavButton.Click -= value;
        }

        /// <summary>
        /// Invoked when add from search action is requested by the user.
        /// </summary>
        public event RoutedEventHandler AddFromSearchRequested
        {
            add => AddFeedFromSearch.Click += value;
            remove => AddFeedFromSearch.Click -= value;
        }
    }
}
