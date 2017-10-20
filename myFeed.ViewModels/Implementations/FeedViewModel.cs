﻿using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedViewModel
    {
        public ObservableCollection<FeedCategoryViewModel> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand OpenSources { get; }
        public ObservableCommand Load { get; }

        public FeedViewModel(
            ICategoriesRepository categoriesRepository,
            INavigationService navigationService,
            IFactoryService factoryService)
        {
            IsEmpty = false;
            IsLoading = true;
            
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            Load = new ObservableCommand(async () =>
            {
                IsEmpty.Value = false;
                IsLoading.Value = true;
                Items.Clear();
                var categories = await categoriesRepository.GetAllAsync();
                foreach (var category in categories)
                    Items.Add(factoryService.CreateInstance<
                        FeedCategoryViewModel>(category));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}