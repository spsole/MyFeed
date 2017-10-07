using System.Collections.ObjectModel;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;
using myFeed.Repositories.Abstractions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FaveViewModel
    {
        public FaveViewModel(
            IDialogService dialogService,
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService)
        {
            Items = new ObservableCollection<ArticleViewModel>();
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(false);
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var articles = await articlesRepository.GetAllAsync();
                Items.Clear();
                foreach (var article in articles)
                {
                    if (!article.Fave) continue;
                    var viewModel = new ArticleViewModel(article,
                        dialogService, settingsService, platformService,
                        navigationService, articlesRepository, translationsService);
                    viewModel.IsFavorite.PropertyChanged += (o, args) =>
                    {
                        if (viewModel.IsFavorite.Value) Items.Add(viewModel);
                        else Items.Remove(viewModel);
                        IsEmpty.Value = Items.Count == 0;
                    };
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Contains favorite items.
        /// </summary>
        public ObservableCollection<ArticleViewModel> Items { get; }

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
        public ObservableCommand Load { get; }
    }
}