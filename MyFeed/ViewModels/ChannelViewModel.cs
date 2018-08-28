using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(ChannelViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class ChannelViewModel
    {
        private readonly Func<Category, ChannelViewModel, ChannelGroupViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;

        public ReactiveCommand<Unit, IEnumerable<Category>> Load { get; }
        public ReactiveList<ChannelGroupViewModel> Categories { get; }
        public ReactiveCommand<Unit, Category> CreateCategory { get; }
        public ReactiveCommand<Unit, Unit> Search { get; }
        
        public string CategoryName { get; set; }
        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }

        public ChannelViewModel(
            Func<Category, ChannelViewModel, ChannelGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager)
        {
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _factory = factory;
            
            Categories = new ReactiveList<ChannelGroupViewModel>();
            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            Load.Select(categories => categories.Select(x => _factory(x, this)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(models => Categories.Clear())
                .Subscribe(Categories.AddRange);
            
            Search = ReactiveCommand.CreateFromTask(_navigationService.Navigate<SearchViewModel>);
            CreateCategory = ReactiveCommand.CreateFromTask(DoCreateCategory,
                this.WhenAnyValue(x => x.CategoryName)
                    .Select(x => !string.IsNullOrWhiteSpace(x)));
            CreateCategory
                .Select(category => string.Empty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CategoryName = x);
            CreateCategory
                .Select(category => _factory(category, this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Categories.Add);

            Load.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Categories.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
            Categories.Changed
                .Throttle(TimeSpan.FromMilliseconds(100)).Skip(1)
                .Select(args => Categories.Select(x => x.Category))
                .Select(_categoryManager.Rearrange)
                .SelectMany(task => task.ToObservable())
                .Subscribe();
        }

        private async Task<Category> DoCreateCategory()
        {
            var category = new Category {Title = CategoryName};
            await _categoryManager.Insert(category);
            return category;
        }
    }
}