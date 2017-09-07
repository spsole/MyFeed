using System;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;
using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;

namespace myFeed.ViewModels.Implementations {
    /// <summary>
    /// Favorites collection view model.
    /// </summary>
    public sealed class FaveViewModel {
        /// <summary>
        /// Instantiates ViewModel.
        /// </summary>
        public FaveViewModel(
            IPlatformProvider platformProvider,
            IArticlesRepository articlesRepository) {

            Items = new ObservableCollection<FeedItemViewModel>();
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(true);

            Load = new ActionCommand(async () => {
                IsLoading.Value = true;
                var articles = await articlesRepository.GetAllAsync();
                Items.Clear();
                foreach (var article in articles) {
                    if (!article.Fave) continue;
                    var viewModel = new FeedItemViewModel(
                        article, platformProvider, articlesRepository);
                    Items.Add(viewModel);
                    viewModel.IsFavorite.PropertyChanged += (o, args) => {
                        if (!viewModel.IsFavorite.Value) {
                            Items.Remove(viewModel);
                            IsEmpty.Value = Items.Count == 0;
                        }
                    };
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Contains favorite items.
        /// </summary>
        public ObservableCollection<FeedItemViewModel> Items { get; }

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Loads favorites collection.
        /// </summary>
        public ActionCommand Load { get; }
    }
}