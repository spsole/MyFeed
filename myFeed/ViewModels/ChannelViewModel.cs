using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        private readonly Func<Category, ChannelViewModel, ChannelGroupViewModel> _factory;
        private readonly IDictionary<ChannelGroupViewModel, Category> _lookup;
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;

        public ReactiveCommand<Unit, IEnumerable<Category>> Load { get; }
        public ReactiveList<ChannelGroupViewModel> Items { get; }
        public Interaction<Unit, string> AddRequest { get; }
        public ReactiveCommand<Unit, Unit> Search { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }

        public ChannelViewModel(
            Func<Category, ChannelViewModel, ChannelGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager)
        {
            _lookup = new Dictionary<ChannelGroupViewModel, Category>();
            Items = new ReactiveList<ChannelGroupViewModel>();
            AddRequest = new Interaction<Unit, string>();
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _factory = factory;

            Add = ReactiveCommand.CreateFromTask(DoAdd);
            Search = ReactiveCommand.CreateFromTask(
                () => _navigationService.Navigate<SearchViewModel>()
            );
            
            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            Load.ObserveOn(RxApp.MainThreadScheduler)
                .Do(categories => Items.Clear())
                .SelectMany(categories => categories)
                .Select(category => (category, _factory(category, this)))
                .Do(tuple => _lookup[tuple.Item2] = tuple.Item1)
                .Select(tuple => tuple.Item2)
                .Subscribe(Items.Add);

            Load.IsExecuting
                .Skip(count: 1)
                .Subscribe(x => IsLoading = x);
            Items.CountChanged
                .Select(count => count == 0)
                .Subscribe(x => IsEmpty = x);
            Items.Changed
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Skip(count: 1)
                .Select(arguments => Items.Select(x => _lookup[x]))
                .SelectMany(_categoryManager.Rearrange)
                .Subscribe();
        }

        private async Task DoAdd()
        {
            var name = await AddRequest.Handle(Unit.Default);
            if (string.IsNullOrWhiteSpace(name)) return;
            var category = new Category {Title = name};
            await _categoryManager.Insert(category);

            var viewModel = _factory(category, this);
            _lookup[viewModel] = category;
            Items.Add(viewModel);
        }
    }
}