using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;
using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FaveViewModel
    {
        public FaveViewModel(
            ISettingsService settingsService,
            IPlatformService platformService,
            IArticlesRepository articlesRepository)
        {
            Items = new ObservableCollection<FeedItemViewModel>();
            IsLoading = ObservableProperty.Of(true);
            IsEmpty = ObservableProperty.Of(true);
            Load = ActionCommand.Of(async () =>
            {
                IsLoading.Value = true;
                var articles = await articlesRepository.GetAllAsync();
                Items.Clear();
                foreach (var article in articles)
                {
                    if (!article.Fave) continue;
                    var viewModel = new FeedItemViewModel(article, 
                        settingsService, platformService, articlesRepository);
                    Items.Add(viewModel);
                    viewModel.IsFavorite.PropertyChanged += (o, args) =>
                    {
                        if (viewModel.IsFavorite.Value) return;
                        Items.Remove(viewModel);
                        IsEmpty.Value = Items.Count == 0;
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