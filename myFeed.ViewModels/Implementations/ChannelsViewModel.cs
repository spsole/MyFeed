using System.Collections.ObjectModel;
using System.Linq;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ChannelsViewModel
    {
        public ChannelsViewModel(
            ICategoriesRepository categoriesRepository,
            ITranslationsService translationsService,
            INavigationService navigationService,
            IFactoryService factoryService,
            IDialogService dialogService)
        {
            IsEmpty = false; 
            IsLoading = true;
            
            Items = new ObservableCollection<ChannelCategoryViewModel>();
            OpenSearch = new ObservableCommand(navigationService.Navigate<SearchViewModel>);
            AddCategory = new ObservableCommand(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                var category = new Category {Title = name};
                await categoriesRepository.InsertAsync(category);
                Items.Add(factoryService.CreateInstance<
                    ChannelCategoryViewModel>(category, this));
            });
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var categories = await categoriesRepository.GetAllAsync();
                Items.Clear();
                foreach (var category in categories)
                    Items.Add(factoryService.CreateInstance<
                        ChannelCategoryViewModel>(category, this));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
                
                Items.CollectionChanged += async (s, a) =>
                {
                    IsEmpty.Value = Items.Count == 0;
                    var items = Items.Select(i => i.Category.Value);
                    await categoriesRepository.RearrangeAsync(items);
                };
            });
        }

        /// <summary>
        /// A collection of inner models.
        /// </summary>
        public ObservableCollection<ChannelCategoryViewModel> Items { get; }

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
        public ObservableCommand AddCategory { get; }

        /// <summary>
        /// Opens search view when user'd like to add
        /// new items to his channel collection.
        /// </summary>
        public ObservableCommand OpenSearch { get; }

        /// <summary>
        /// Loads categories into view.
        /// </summary>
        public ObservableCommand Load { get; }
    }
}