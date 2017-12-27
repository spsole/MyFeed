﻿using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(FeedViewModel))]
    public sealed class FeedViewModel
    {
        public ObservableCollection<FeedCategoryViewModel> Items { get; }

        public ObservableProperty<FeedCategoryViewModel> Selected { get; }
        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand OpenSources { get; }
        public ObservableCommand Load { get; }

        public FeedViewModel(
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFactoryService factoryService)
        {
            (IsEmpty, IsLoading) = (false, true);
            Selected = new ObservableProperty<FeedCategoryViewModel>();
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            Load = new ObservableCommand(async () =>
            {
                IsEmpty.Value = false;
                IsLoading.Value = true;
                Items.Clear();
                var categories = await categoryManager.GetAllAsync();
                foreach (var category in categories)
                    Items.Add(factoryService.CreateInstance<
                        FeedCategoryViewModel>(category));
                Selected.Value = Items.FirstOrDefault();
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}