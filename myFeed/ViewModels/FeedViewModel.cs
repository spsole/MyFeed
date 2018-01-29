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
            INavigationService navigationService,
            ICategoryManager categoryManager,
            ISettingManager settingManager,
            IFactoryService factoryService)
        {
            IsLoading = true;
            Items = new ReactiveList<FeedGroupViewModel>();
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                (IsEmpty, IsLoading) = (false, true);
                var settings = await settingManager.Read();
                var categories = await categoryManager.GetAllAsync();
                var factory = factoryService.Create<Func<Category, FeedGroupViewModel>>();
                var viewModels = categories.Select(x => factory(x));
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