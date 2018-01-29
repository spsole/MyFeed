using System;
using System.Collections.Generic;
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
    [ExportEx(typeof(ChannelViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class ChannelViewModel
    {
        public ReactiveList<ChannelGroupViewModel> Items { get; }
        public ReactiveCommand Search { get; }
        public ReactiveCommand Load { get; }
        public ReactiveCommand Add { get; }

        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public ChannelViewModel(
            ITranslationService translationsService,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFactoryService factoryService,
            IDialogService dialogService)
        {
            IsLoading = true;
            Items = new ReactiveList<ChannelGroupViewModel>();
            Search = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<SearchViewModel>());
            var factory = factoryService.Create<Func<Category, ChannelViewModel, ChannelGroupViewModel>>();
            var map = new Dictionary<ChannelGroupViewModel, Category>();
            Add = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve(Constants.EnterNameOfNewCategory),
                    translationsService.Resolve(Constants.EnterNameOfNewCategoryTitle));
                if (string.IsNullOrWhiteSpace(name)) return;
                var category = new Category {Title = name};
                await categoryManager.InsertAsync(category);
                var viewModel = factory(category, this);
                map[viewModel] = category;
                Items.Add(viewModel);
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                map.Clear();
                IsLoading = true;
                var categories = await categoryManager.GetAllAsync();
                foreach (var category in categories) map[factory(category, this)] = category;
                Items.AddRange(map.Keys);
                IsEmpty = Items.Count == 0;
                IsLoading = false;
                Items.Changed.Subscribe(async x =>
                {
                    IsEmpty = Items.Count == 0;
                    var items = Items.Select(i => map[i]);
                    await categoryManager.RearrangeAsync(items);
                });
            });
        }
    }
}