using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace myFeed.Search
{
    public sealed partial class SearchPage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SearchPageViewModel),
                typeof(SearchPage),
                new PropertyMetadata(null)
            );

        public SearchPageViewModel ViewModel
        {
            get => (SearchPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public SearchPage()
        {
            ViewModel = new SearchPageViewModel();
            InitializeComponent();
        }

        private void ShowFlyout(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((Grid)sender);

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            Focus(FocusState.Programmatic);
            ViewModel.FetchAsync();
        }
    }
}
