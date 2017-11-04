using System.Collections.ObjectModel;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchViewModel
    {
        public ObservableCollection<SearchItemViewModel> Items { get; }

        public ObservableProperty<string> SearchQuery { get; }
        public ObservableProperty<bool> IsGreeting { get; }
        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand Fetch { get; }

        public SearchViewModel(
            IFactoryService factoryService,
            ISearchService searchService)
        {
            IsGreeting = true;
            SearchQuery = string.Empty;
            (IsEmpty, IsLoading) = (false, false);
            
            Items = new ObservableCollection<SearchItemViewModel>();
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var query = SearchQuery.Value;
                var searchResults = await searchService.SearchAsync(query);
                IsGreeting.Value = false;
                Items.Clear();
                foreach (var feedlyItem in searchResults.Results)
                    Items.Add(factoryService.CreateInstance<
                        SearchItemViewModel>(feedlyItem));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}