using System;
using System.Linq;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FeedViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedViewModel
    {
        public ReactiveList<FeedGroupViewModel> Items { get; }
        public FeedGroupViewModel Selection { get; set; }
        public ReactiveCommand Modify { get; }
        public ReactiveCommand Load { get; }

        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }
        public bool Images { get; private set; }

        public FeedViewModel(
            Func<Category, FeedGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            ISettingManager settingManager)
        {
            IsLoading = true;
            Items = new ReactiveList<FeedGroupViewModel>();
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                IsEmpty = false;
                IsLoading = true;
                var settings = await settingManager.Read();
                var categories = await categoryManager.GetAllAsync();
                var viewModels = categories.Select(factory);
                Items.Clear();
                Items.AddRange(viewModels);
                Selection = Items.FirstOrDefault();
                Images = settings.Images;
                IsEmpty = Items.Count == 0;
                IsLoading = false;
            });
        }
    }
}