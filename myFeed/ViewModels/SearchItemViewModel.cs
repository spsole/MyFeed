using System;
using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SearchItemViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SearchItemViewModel
    {
        private readonly IPlatformService _platformService;
        private readonly FeedlyItem _feedlyItem;
         
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public Interaction<Unit, bool> Copied { get; }
        public bool IsSelected { get; set; }  
        
        public string Description => _feedlyItem.Description;
        public string Image => _feedlyItem.IconUrl;
        public string Title => _feedlyItem.Title;
        public string Feed => _feedlyItem.FeedId;
        public string Url => _feedlyItem.Website;
        
        public SearchItemViewModel(
            IPlatformService platformService,
            FeedlyItem feedlyItem)
        {
            _feedlyItem = feedlyItem;
            _platformService = platformService;
            Open = ReactiveCommand.CreateFromTask(
                () => _platformService.LaunchUri(new Uri(Url)),
                Observable.Return(Uri.IsWellFormedUriString(Url, UriKind.Absolute)));

            Copy = ReactiveCommand.CreateFromTask(
                () => _platformService.CopyTextToClipboard(Url),
                Observable.Return(!string.IsNullOrWhiteSpace(Url)));
            
            Copied = new Interaction<Unit, bool>();
            Copy.ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(Copied.Handle)
                .Subscribe();
        }
    }
}