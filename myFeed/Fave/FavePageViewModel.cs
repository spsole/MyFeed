using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using myFeed.Extensions.Mvvm;
using myFeed.Extensions.Mvvm.Implementation;

namespace myFeed.Fave
{
    /// <summary>
    /// Favorites collection view model.
    /// </summary>
    public class FavePageViewModel : DeeplyObservableCollection<FaveItemViewModel>
    {
        /// <summary>
        /// Initializes a new instance of FavePageViewModel.
        /// </summary>
        public FavePageViewModel() => LoadCollectionAsync();

        #region Properties

        /// <summary>
        /// Indicates if fetcher is loading data right now or not.
        /// </summary>
        public IObservableProperty<bool> IsLoading { get; } = 
            new ObservableProperty<bool>(true);

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public IObservableProperty<bool> IsEmpty { get; } = 
            new ObservableProperty<bool>(false);

        #endregion

        #region Methods

        /// <summary>
        /// Pins the entire page to start screen.
        /// </summary>
        public async void PinToStart()
        {
            const string tileTitle = "Favorites";
            var secondaryTile = new SecondaryTile(tileTitle, tileTitle, "fav",
                new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"),
                TileSize.Square150x150
            );
            await secondaryTile.RequestCreateAsync();
        }

        /// <summary>
        /// Loads the entire collection from files to UI.
        /// </summary>
        public async void LoadCollectionAsync()
        {
            // Slight delay.
            await Task.Delay(300);
            var manager = FaveManager.GetInstance();
            var articles = await manager.LoadArticles();
            articles.ToList().ForEach(Add);

            // Hide load screen.
            IsEmpty.Value = Count == 0;
            IsLoading.Value = false;
        }

        /// <summary>
        /// Deleted given item from the collection.
        /// </summary>
        /// <param name="model"></param>
        public async void DeleteItem(FaveItemViewModel model)
        {
            // Delete from disk.
            var manager = FaveManager.GetInstance();
            var index = IndexOf(model);
            await manager.DeleteArticle(model, index);
            RemoveAt(index);

            // Toggle empty control if needed.
            IsEmpty.Value = Count == 0;
        }

        #endregion
    }
}
