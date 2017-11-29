using System.Collections.ObjectModel;
using System.Linq;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ChannelsViewModel
    {
        public ObservableCollection<ChannelCategoryViewModel> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand AddCategory { get; }
        public ObservableCommand OpenSearch { get; }
        public ObservableCommand Load { get; }

        public ChannelsViewModel(
            ICategoryStoreService categoriesRepository,
            ITranslationsService translationsService,
            INavigationService navigationService,
            IFactoryService factoryService,
            IDialogService dialogService)
        {
            (IsEmpty, IsLoading) = (false, true); 
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
                
                // Subscribe on collection changed to perform items rearranging.
                Items.CollectionChanged += async (s, a) =>
                {
                    IsEmpty.Value = Items.Count == 0;
                    var items = Items.Select(i => i.Category.Value);
                    await categoriesRepository.RearrangeAsync(items);
                };
            });
        }
    }
}