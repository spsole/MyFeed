using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FeedViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedViewModel : ISupportsActivation
    {
        private readonly Func<Category, FeedGroupViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;
        
        public ReactiveCommand<Unit, Unit> Modify { get; }
        public ReactiveCommand<Unit, IEnumerable<Category>> Load { get; }
        public ReactiveList<FeedGroupViewModel> Items { get; }
        public FeedGroupViewModel Selection { get; set; }
        public ViewModelActivator Activator { get; }

        public bool IsLoading { get; private set; } = true;
        public bool HasErrors { get; private set; }
        public bool IsEmpty { get; private set; }

        public FeedViewModel(
            Func<Category, FeedGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager)
        {
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _factory = factory;

            Items = new ReactiveList<FeedGroupViewModel>();
            Modify = ReactiveCommand.CreateFromTask(_navigationService.Navigate<ChannelViewModel>);
            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            Load.Select(categories => categories.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(models => Items.Clear())
                .Subscribe(Items.AddRange);

            Load.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Items.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
            Items.Changed
                .Select(args => Items.FirstOrDefault())
                .Where(selection => selection != null)
                .Subscribe(x => Selection = x);
            
            Load.IsExecuting
                .Where(executing => executing)
                .Select(executing => false)
                .Subscribe(x => HasErrors = x);
            Load.ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HasErrors = x);

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
                Load.Execute().Subscribe());
        }
    }
}