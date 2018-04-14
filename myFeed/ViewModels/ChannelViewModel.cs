using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
        public Interaction<Unit, string> AddRequest { get; }
        
        public ReactiveCommand<Unit, Unit> Search { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }

        public ChannelViewModel(
            Func<Category, ChannelGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IMessageBus messageBus)
        {
            AddRequest = new Interaction<Unit, string>();
            Items = new ReactiveList<ChannelGroupViewModel>();
            var map = new Dictionary<ChannelGroupViewModel, Category>();
            messageBus.Listen<ChannelGroupViewModel>()
                      .Subscribe(x => Items.Remove(x));
                
            Search = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<SearchViewModel>());
            Add = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await AddRequest.Handle(Unit.Default);
                if (string.IsNullOrWhiteSpace(name)) return;
                var category = new Category {Title = name};
                await categoryManager.InsertAsync(category);
                var viewModel = factory(category);
                map[viewModel] = category;
                Items.Add(viewModel);
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                map.Clear();
                IsLoading = true;
                var categories = await categoryManager.GetAllAsync();
                foreach (var category in categories) map[factory(category)] = category;
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