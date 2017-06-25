using System.Net.Http;
using System.Threading.Tasks;
using myFeed.Extensions;
using myFeed.Extensions.Mvvm;
using myFeed.Extensions.Mvvm.Implementation;
using myFeed.FeedModels.Models.Feedly;
using Newtonsoft.Json;

namespace myFeed.Search
{
    /// <summary>
    /// Search page view model.
    /// </summary>
    public class SearchPageViewModel : DeeplyObservableCollection<SearchItemViewModel>
    {
        #region Properties

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public IObservableProperty<bool> IsLoading { get; } = 
            new ObservableProperty<bool>(false);

        /// <summary>
        /// Is welcome screen visible or not?
        /// </summary>
        public IObservableProperty<bool> IsWelcomeVisible { get; } = 
            new ObservableProperty<bool>(true);

        /// <summary>
        /// Is collection empty or not?
        /// </summary>
        public IObservableProperty<bool> IsEmpty { get; } = 
            new ObservableProperty<bool>(false);

        /// <summary>
        /// Contains search query.
        /// </summary>
        public IObservableProperty<string> SearchQuery { get; } = 
            new ObservableProperty<string>(string.Empty);

        #endregion

        #region Methods

        /// <summary>
        /// Fetches results from Feedly search engine.
        /// </summary>
        public async void FetchAsync()
        {
            // Toggle loading UI on.
            IsLoading.Value = true;

            // Build search uri.
            var queryString = SearchQuery;
            var queryUrl = 
                @"http://cloud.feedly.com/v3/search/feeds?count=40&query=:" + 
                queryString.Value;

            // Retrieve data async.
            using (var client = new HttpClient())
            using (var response = await Tools.TryAsync(client.GetAsync(queryUrl), null))
            {
                if (response != null)
                {
                    // Read content string.
                    var responseString = await response.Content.ReadAsStringAsync();
                    Tools.Log($"Received: {responseString}");
                    Clear();

                    // Refill collection.
                    var rootSearchObject = JsonConvert.DeserializeObject<SearchRootModel>(responseString);
                    rootSearchObject.Results.ForEach(i => Add(new SearchItemViewModel(i)));
                }
            }

            // Disallow instant clicking.
            await Task.Delay(500);

            // Toggle loading indicator off.
            IsEmpty.Value = Count == 0;
            IsWelcomeVisible.Value = false;
            IsLoading.Value = false;
        }

        #endregion
    }
}
