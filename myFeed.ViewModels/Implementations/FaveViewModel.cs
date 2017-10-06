using System.Linq;
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
            Items = new Collection<ArticleViewModel>();
            IsLoading = new Property<bool>(true);
            IsEmpty = new Property<bool>(false);
            Load = new Command(async () =>
            {
                IsLoading.Value = true;
                var articles = await articlesRepository.GetAllAsync();
                var articleViewModels = articles
                    .Where(i => i.Fave)
                    .Select(i => new ArticleViewModel(i, 
                        dialogService, settingsService, platformService, 
                        navigationService, articlesRepository, translationsService));
                Items.Clear();
                Items.AddRange(articleViewModels);

                foreach (var viewModel in Items) viewModel.IsFavorite.PropertyChanged += (o, args) =>
                {
                    if (viewModel.IsFavorite.Value) Items.Add(viewModel);
                    else Items.Remove(viewModel);
                    IsEmpty.Value = Items.Count == 0;
                };
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Contains favorite items.
        /// </summary>
        public Collection<ArticleViewModel> Items { get; }

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public Property<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public Property<bool> IsEmpty { get; }

        /// <summary>
        /// Loads favorites collection.
        /// </summary>
        public Command Load { get; }
    }
}