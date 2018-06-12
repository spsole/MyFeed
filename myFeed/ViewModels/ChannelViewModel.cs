using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Events;
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
        private readonly IDictionary<ChannelGroupViewModel, Category> _lookup;
        private readonly Func<Category, ChannelGroupViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;
        private readonly IMessageBus _messageBus;

        public ReactiveList<ChannelGroupViewModel> Items { get; }
        public Interaction<Unit, string> AddRequest { get; }
        public ReactiveCommand<Unit, Unit> Search { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; } = true;

        public ChannelViewModel(
            Func<Category, ChannelGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IMessageBus messageBus)
        {
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _messageBus = messageBus;
            _factory = factory;

            AddRequest = new Interaction<Unit, string>();
            Items = new ReactiveList<ChannelGroupViewModel>();
            Add = ReactiveCommand.CreateFromTask(DoAdd);

            _lookup = new Dictionary<ChannelGroupViewModel, Category>();
            _messageBus.Listen<CategoryDeleteEvent>()
                .Do(x => Items.Remove(x.ChannelGroupViewModel))
                .SelectMany(x => _categoryManager.Remove(x.Category))
                .Subscribe();

            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Search = ReactiveCommand.CreateFromTask(
                () => _navigationService.Navigate<SearchViewModel>()
            );

            Load.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Items.CountChanged
                .Select(count => count == 0)
                .Subscribe(x => IsEmpty = x);
            
            Items.Changed.Skip(1)
                .Select(x => Items)
                .Select(x => x.Select(i => _lookup[i]))
                .SelectMany(_categoryManager.Rearrange)
                .Subscribe();
        }

        private async Task DoLoad()
        {
            _lookup.Clear();
            var categories = await _categoryManager.GetAll();
            categories.ToList().ForEach(x => _lookup[_factory(x)] = x);
            Items.AddRange(_lookup.Keys);
        }

        private async Task DoAdd()
        {
            var name = await AddRequest.Handle(Unit.Default);
            if (string.IsNullOrWhiteSpace(name)) return;
            var category = new Category { Title = name };
            await _categoryManager.Insert(category);

            var viewModel = _factory(category);
            _lookup[viewModel] = category;
            Items.Add(viewModel);
        }
    }
}