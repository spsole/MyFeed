using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace myFeed.Sources
{
    public sealed partial class SourcesPage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SourcesPageViewModel),
                typeof(SourcesPage),
                new PropertyMetadata(null)
            );

        public SourcesPageViewModel ViewModel
        {
            get => (SourcesPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public SourcesPage()
        {
            ViewModel = new SourcesPageViewModel();
            InitializeComponent();
            Navigation.NavigationManager.GetInstance().AddBackHandler(a =>
            {
                if (!ViewModel.IsRearrangeEnabledProperty.Value) return;
                ViewModel.IsRearrangeEnabledProperty.Value = false;
                a.Handled = true;
            });
        }

        private void ShowFlyout(object sender, RoutedEventArgs e) => 
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void OnUnloaded(object sender, RoutedEventArgs e) =>
            ViewModel.IsRearrangeEnabledProperty.Value = false;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => 
            ((ListView)sender).SelectedItem = null;
    }
}
