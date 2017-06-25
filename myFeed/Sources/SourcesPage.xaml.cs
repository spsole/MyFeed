using Windows.System;
using Windows.UI.Xaml;
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
        
        private void OnUnloaded(object sender, RoutedEventArgs e) => ViewModel.IsRearrangeEnabledProperty.Value = false;

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            ((SourceCategoryViewModel) ((FrameworkElement) sender).DataContext).AddSource();
        }
    }
}
