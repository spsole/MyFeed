using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Navigation
{
    public sealed partial class NavigationPage
    {
        public MenuViewModel ViewModel => DataContext as MenuViewModel;

        public static Frame NavigationFrame { get; private set; }

        public NavigationPage()
        {
            // Init page.
            InitializeComponent();
            NavigationFrame = MainFrame;
            
            var manager = NavigationManager.GetInstance();
            manager.AddBackHandler(args =>
            {
                // Go back using root frame.
                if (!Frame.CanGoBack) return;
                Frame.GoBack();
                args.Handled = true;
            });
            manager.AddBackHandler(args =>
            {
                // Go back using internal frame.
                if (!MainFrame.CanGoBack) return;
                MainFrame.GoBack();
                args.Handled = true;
            });
        }

        private void OpenMenu(object sender, RoutedEventArgs e) =>
            SplitView.IsSwipeablePaneOpen = !SplitView.IsSwipeablePaneOpen;
    }
}
