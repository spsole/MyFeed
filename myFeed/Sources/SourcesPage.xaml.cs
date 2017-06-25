using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace myFeed.Sources
{
    public sealed partial class SourcesPage
    {
        public SourcesPageViewModel ViewModel => DataContext as SourcesPageViewModel;

        public SourcesPage()
        {
            InitializeComponent();
            Navigation.NavigationManager.GetInstance().AddBackHandler(a =>
            {
                if (!ViewModel.IsRearrangeEnabledProperty.Value) return;
                ViewModel.IsRearrangeEnabledProperty.Value = false;
                a.Handled = true;
            });
        }

        private void ShowFlyout(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void OnUnloaded(object sender, RoutedEventArgs e) => ViewModel.IsRearrangeEnabledProperty.Value = false;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ((ListView)sender).SelectedItem = null;

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            ((SourceCategoryViewModel) ((FrameworkElement) sender).DataContext).AddSource();
        }
    }
}
