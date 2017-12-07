using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;
using DryIocAttributes;
using System.Linq;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(ChannelsViewModel))]
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
            var viewModelToModelMap = new Dictionary<ChannelCategoryViewModel, Category>();

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
                var viewModel = factoryService.CreateInstance<
                    ChannelCategoryViewModel>(category, this);
                viewModelToModelMap[viewModel] = category;
                Items.Add(viewModel);
            });
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var categories = await categoriesRepository.GetAllAsync();
                foreach (var category in categories)
                {
                    var viewModel = factoryService.CreateInstance<
                        ChannelCategoryViewModel>(category, this);
                    viewModelToModelMap[viewModel] = category;
                    Items.Add(viewModel);
                }

                IsLoading.Value = false;
                IsEmpty.Value = Items.Count == 0;
                Items.CollectionChanged += async (s, a) =>
                {
                    IsEmpty.Value = Items.Count == 0;
                    var items = Items.Select(i => viewModelToModelMap[i]);
                    await categoriesRepository.RearrangeAsync(items);
                };
            });
        }
    }
}