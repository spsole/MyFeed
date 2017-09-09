using System.Collections.ObjectModel;
using System.Linq;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    /// <summary>
    /// Sources page ViewModel.
    /// </summary>
    public sealed class SourcesViewModel
    {
        /// <summary>
        /// Instantiates new ViewModel.
        /// </summary>
        public SourcesViewModel(
            IPlatformProvider platformProvider,
            ISourcesRepository sourcesRepository,
            ITranslationsService translationsService)
        {
            Items = new ObservableCollection<SourcesCategoryViewModel>();
            IsEmpty = new ObservableProperty<bool>(false);
            IsLoading = new ObservableProperty<bool>(true);
            Items.CollectionChanged += (s, a) =>
            {
                IsEmpty.Value = Items.Count == 0;
                var items = Items.Select(i => i.Category.Value);
                sourcesRepository.RearrangeAsync(items);
            };

            Load = new ActionCommand(async () =>
            {
                IsLoading.Value = true;
                var categories = await sourcesRepository.GetAllAsync();
                Items.Clear();
                foreach (var category in categories)
                {
                    var viewModel = new SourcesCategoryViewModel(category, this,
                        sourcesRepository, translationsService, platformProvider);
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
            AddCategory = new ActionCommand(async () =>
            {
                var name = await platformProvider.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                var category = new SourceCategoryEntity {Title = name};
                await sourcesRepository.InsertAsync(category);
                var viewModel = new SourcesCategoryViewModel(category, this,
                    sourcesRepository, translationsService, platformProvider);
                Items.Add(viewModel);
            });
        }

        /// <summary>
        /// A collection of inner models.
        /// </summary>
        public ObservableCollection<SourcesCategoryViewModel> Items { get; }

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if welcome screen is visible.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Loads categories into view.
        /// </summary>
        public ActionCommand Load { get; }

        /// <summary>
        /// Adds new category to list.
        /// </summary>
        public ActionCommand AddCategory { get; }
    }
}