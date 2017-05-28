using Windows.UI.Xaml;

namespace myFeed.Search.Controls
{
    public sealed partial class NothingControl
    {
        public NothingControl() => InitializeComponent();

        /// <summary>
        /// Forwards given event to button.
        /// </summary>
        public event RoutedEventHandler RefreshRequested
        {
            add => RefreshButton.Click += value;
            remove => RefreshButton.Click -= value;
        }
    }
}
