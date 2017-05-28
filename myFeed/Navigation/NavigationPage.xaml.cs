using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Navigation
{
    public sealed partial class NavigationPage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(MenuViewModel),
                typeof(NavigationPage),
                new PropertyMetadata(null)
            );

        public MenuViewModel ViewModel
        {
            get => (MenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static Frame NavigationFrame { get; private set; }

        public NavigationPage()
        {
            // Init page.
            ViewModel = new MenuViewModel();
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
