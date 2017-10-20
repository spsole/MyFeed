using System.Collections.ObjectModel;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchViewModel
    {
        public SearchViewModel(
            IFactoryService factoryService,
            ISearchService searchService)
        {
            IsEmpty = true;
            IsLoading = false;
            IsGreeting = true;
            SearchQuery = string.Empty;
            
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

        /// <summary>
        /// Loaded models.
        /// </summary>
        public ObservableCollection<SearchItemViewModel> Items { get; }

        /// <summary>
        /// Contains search query.
        /// </summary>
        public ObservableProperty<string> SearchQuery { get; }

        /// <summary>
        /// Indicates if ViewModel is showing greeting.
        /// </summary>
        public ObservableProperty<bool> IsGreeting { get; }

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Is collection empty or not?
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Fetches results from Feedly search engine.
        /// </summary>
        public ObservableCommand Fetch { get; }
    }
}