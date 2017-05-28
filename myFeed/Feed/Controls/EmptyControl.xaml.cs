using Windows.UI.Xaml;

namespace myFeed.Feed.Controls
{
    public sealed partial class EmptyControl
    {
        public EmptyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when refresh is requested by the user.
        /// </summary>
        public event RoutedEventHandler RefreshRequested
        {
            add => RefreshButton.Click += value;
            remove => RefreshButton.Click -= value;
        }
    }
}
