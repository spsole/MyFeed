using System.Collections.ObjectModel;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchViewModel
    {
        public SearchViewModel(
            ISourcesRepository sourcesRepository,
            IPlatformService platformService,
            IDialogService dialogService,
            ISearchService searchService)
        {
            Items = new ObservableCollection<SearchItemViewModel>();
            SearchQuery = ObservableProperty.Of(string.Empty);
            IsLoading = ObservableProperty.Of(false);
            IsEmpty = ObservableProperty.Of(true);
            Fetch = ActionCommand.Of(async () =>
            {
                IsLoading.Value = true;
                var query = SearchQuery.Value;
                var searchResults = await searchService.Search(query);
                Items.Clear();
                foreach (var result in searchResults.Results)
                    Items.Add(new SearchItemViewModel(result, 
                        dialogService, platformService, sourcesRepository));
                await Task.Delay(500);
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
        public ActionCommand Fetch { get; }
    }
}