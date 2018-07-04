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
        public ReactiveList<ChannelGroupViewModel> Categories { get; }
        public ReactiveCommand<Unit, Unit> Search { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }
        
        public string CategoryName { get; set; }
        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }

        public ChannelViewModel(
            Func<Category, ChannelViewModel, ChannelGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager)
        {
            _lookup = new Dictionary<ChannelGroupViewModel, Category>();
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _factory = factory;
            
            Categories = new ReactiveList<ChannelGroupViewModel>();
            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            Load.ObserveOn(RxApp.MainThreadScheduler)
                .Do(categories => Categories.Clear())
                .SelectMany(categories => categories)
                .Select(category => (category, _factory(category, this)))
                .Do(tuple => _lookup[tuple.Item2] = tuple.Item1)
                .Select(tuple => tuple.Item2)
                .Subscribe(Categories.Add);
            
            Search = ReactiveCommand.CreateFromTask(() => 
                _navigationService.Navigate<SearchViewModel>());
            Add = ReactiveCommand.CreateFromTask(DoAdd,
                this.WhenAnyValue(x => x.CategoryName)
                    .Select(x => !string.IsNullOrWhiteSpace(x)));

            Load.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Categories.CountChanged
                .Select(count => count == 0)
                .Subscribe(x => IsEmpty = x);
            Categories.Changed
                .Throttle(TimeSpan.FromMilliseconds(100)).Skip(1)
                .Select(arguments => Categories.Select(x => _lookup[x]))
                .SelectMany(_categoryManager.Rearrange)
                .Subscribe();
        }

        private async Task DoAdd()
        {
            var category = new Category {Title = CategoryName};
            await _categoryManager.Insert(category);
            var viewModel = _factory(category, this);
            _lookup[viewModel] = category;
            Categories.Add(viewModel);
            CategoryName = null;
        }
    }
}