using System.Linq;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SourcesViewModel
    {
        public SourcesViewModel(
            IDialogService dialogService,
            IPlatformService platformService,
            ISourcesRepository sourcesRepository,
            INavigationService navigationService,
            ITranslationsService translationsService)
        {
            IsEmpty = new Property<bool>(false);
            IsLoading = new Property<bool>(true);
            Items = new Collection<SourcesCategoryViewModel>();
            OpenSearch = new Command(navigationService.Navigate<SearchViewModel>);
            Load = new Command(async () =>
            {
                IsLoading.Value = true;
                var categories = await sourcesRepository.GetAllAsync();
                var categoryViewModels = categories
                    .Select(i => new SourcesCategoryViewModel(i,
                        this, dialogService, platformService,
                        sourcesRepository, translationsService));

                Items.Clear();
                Items.AddRange(categoryViewModels);
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;

                Items.CollectionChanged += async (s, a) =>
                {
                    IsEmpty.Value = Items.Count == 0;
                    var items = Items.Select(i => i.Category.Value);
                    await sourcesRepository.RearrangeAsync(items);
                };
            });
            AddCategory = new Command(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                var category = new SourceCategoryEntity {Title = name};
                await sourcesRepository.InsertAsync(category);
                Items.Add(new SourcesCategoryViewModel(category, 
                    this, dialogService, platformService, 
                    sourcesRepository, translationsService));
            });
        }

        /// <summary>
        /// A collection of inner models.
        /// </summary>
        public Collection<SourcesCategoryViewModel> Items { get; }

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public Property<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if welcome screen is visible.
        /// </summary>
        public Property<bool> IsEmpty { get; }

        /// <summary>
        /// Adds new category to list.
        /// </summary>
        public Command AddCategory { get; }

        /// <summary>
        /// Opens search view when user'd like to add
        /// new items to his channel collection.
        /// </summary>
        public Command OpenSearch { get; }

        /// <summary>
        /// Loads categories into view.
        /// </summary>
        public Command Load { get; }
    }
}