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
            INavigationService navigationService,
            IArticlesRepository articlesRepository)
        {
            Items = new ObservableCollection<ArticleViewModel>();
            IsLoading = new Property<bool>(true);
            IsEmpty = new Property<bool>(false);
            Load = new Command(async () =>
            {
                IsLoading.Value = true;
                var articles = await articlesRepository.GetAllAsync();
                Items.Clear();
                foreach (var article in articles)
                {
                    if (!article.Fave) continue;
                    var viewModel = new ArticleViewModel(article, settingsService, 
                        platformService, navigationService, articlesRepository);
                    Items.Add(viewModel);
                    viewModel.IsFavorite.PropertyChanged += (o, args) =>
                    {
                        if (viewModel.IsFavorite.Value) Items.Add(viewModel);
                        else Items.Remove(viewModel);
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
        public ObservableCollection<ArticleViewModel> Items { get; }

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