using System;
using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FeedItemFullViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedItemFullViewModel
    {
        private readonly ISettingManager _settingManager;

        public ReactiveCommand<Unit, Settings> Load { get; }
        public FeedItemViewModel Article { get; }

        public double Font { get; private set; }
        public bool Images { get; private set; }

        public FeedItemFullViewModel(
            FeedItemViewModel feedItemViewModel,
            ISettingManager settingManager)
        {
            Article = feedItemViewModel;
            _settingManager = settingManager;

            Load = ReactiveCommand.CreateFromTask(_settingManager.Read);
            Load.ObserveOn(RxApp.MainThreadScheduler)
                .Select(settings => settings.Images)
                .Subscribe(x => Images = x);
            Load.ObserveOn(RxApp.MainThreadScheduler)
                .Select(settings => settings.Font)
                .Subscribe(x => Font = x);
        }
    }
}
