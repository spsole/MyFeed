using System.Collections.ObjectModel;
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
            ITranslationsService translationsService)
        {
            Items = new ObservableCollection<SourcesCategoryViewModel>();
            IsLoading = ObservableProperty.Of(true);
            IsEmpty = ObservableProperty.Of(false);
            Items.CollectionChanged += (s, a) =>
            {
                IsEmpty.Value = Items.Count == 0;
                var items = Items.Select(i => i.Category.Value);
                sourcesRepository.RearrangeAsync(items);
            };
            Load = ActionCommand.Of(async () =>
            {
                IsLoading.Value = true;
                var categories = await sourcesRepository.GetAllAsync();
                Items.Clear();
                foreach (var category in categories)
                    Items.Add(new SourcesCategoryViewModel(category, 
                        this, dialogService, platformService, 
                        sourcesRepository, translationsService));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
            AddCategory = ActionCommand.Of(async () =>
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
        /// Adds new category to list.
        /// </summary>
        public ActionCommand AddCategory { get; }

        /// <summary>
        /// Loads categories into view.
        /// </summary>
        public ActionCommand Load { get; }
    }
}