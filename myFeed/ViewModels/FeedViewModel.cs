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
    [ExportEx(typeof(FeedViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedViewModel
    {
        private readonly Func<Category, FeedGroupViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;
        private readonly ISettingManager _settingManager;
        
        public ReactiveCommand<Unit, IEnumerable<Category>> Load { get; }
        public ReactiveList<FeedGroupViewModel> Items { get; }
        public FeedGroupViewModel Selection { get; set; }

        public Interaction<Exception, bool> Error { get; }
        public ReactiveCommand<Unit, Unit> Modify { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public bool Images { get; private set; }

        public FeedViewModel(
            Func<Category, FeedGroupViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            ISettingManager settingManager)
        {
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _settingManager = settingManager;
            _factory = factory;

            Items = new ReactiveList<FeedGroupViewModel>();
            Modify = ReactiveCommand.CreateFromTask(
                () => _navigationService.Navigate<ChannelViewModel>()
            );

            Load = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            Load.Select(categories => categories.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(models => Items.Clear())
                .Subscribe(Items.AddRange);
            Load.IsExecuting
                .Where(executing => executing)
                .SelectMany(x => _settingManager.Read())
                .Select(settings => settings.Images)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Images = x);

            Items.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
            Load.IsExecuting
                .Skip(1)
                .Subscribe(x => IsLoading = x);
            Items.Changed
                .Select(args => Items.FirstOrDefault())
                .Where(selection => selection != null)
                .Subscribe(x => Selection = x);

            Error = new Interaction<Exception, bool>();
            Load.ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(Error.Handle)
                .Where(retry => retry)
                .Select(x => Unit.Default)
                .InvokeCommand(Load);
        }
    }
}