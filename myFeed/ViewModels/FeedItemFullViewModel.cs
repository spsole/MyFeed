using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
            _settingManager = settingManager;
            Article = feedItemViewModel;
            Load = ReactiveCommand.CreateFromTask(_settingManager.Read);
            Load.ObserveOn(RxApp.MainThreadScheduler)
                .Do(settings => Images = settings.Images)
                .Do(settings => Font = settings.Font)
                .Subscribe();
        }
    }
}
