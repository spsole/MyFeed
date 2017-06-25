using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace myFeed.Search
{
    public sealed partial class SearchPage
    {
        public SearchPageViewModel ViewModel => DataContext as SearchPageViewModel;

        public SearchPage() => InitializeComponent();

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            Focus(FocusState.Programmatic);
            ViewModel.FetchAsync();
        }
    }
}
