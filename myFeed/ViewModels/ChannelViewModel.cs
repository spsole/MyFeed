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

        public ReactiveCommand<Unit, IEnumerable<Category>> Load { get; }
        public ReactiveList<ChannelGroupViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> Search { get; }
        
        public Interaction<Unit, string> AddRequest { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsLoaded { get; private set; } = false;
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

            Items = new ReactiveList<ChannelGroupViewModel>();
            AddRequest = new Interaction<Unit, string>();
            Add = ReactiveCommand.CreateFromTask(DoAdd);
            Search = ReactiveCommand.CreateFromTask(
                () => _navigationService.Navigate<SearchViewModel>()
            );

            _lookup = new Dictionary<ChannelGroupViewModel, Category>();
            _messageBus.Listen<CategoryDeleteEvent>()
                .Do(message => Items.Remove(message.ChannelGroupViewModel))
                .SelectMany(message => _categoryManager.Remove(message.Category))
                .Subscribe();

            var isLoaded = this.WhenAnyValue(x => x.IsLoaded);
            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll, isLoaded);
            Load.SelectMany(categories => categories)
                .Select(category => (category, _factory(category)))
                .Do(tuple => _lookup[tuple.Item2] = tuple.Item1)
                .Select(tuple => tuple.Item2)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(model => IsLoaded = true)
                .Subscribe(Items.Add);

            Load.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Items.CountChanged
                .Select(count => count == 0)
                .Subscribe(x => IsEmpty = x);
            Items.Changed.Skip(1)
                .Select(arguments => Items)
                .Select(items => items.Select(x => _lookup[x]))
                .SelectMany(_categoryManager.Rearrange)
                .Subscribe();
        }

        private async Task DoAdd()
        {
            var name = await AddRequest.Handle(Unit.Default);
            if (string.IsNullOrWhiteSpace(name)) return;
            var category = new Category {Title = name};
            await _categoryManager.Insert(category);

            var viewModel = _factory(category);
            _lookup[viewModel] = category;
            Items.Add(viewModel);
        }
    }
}