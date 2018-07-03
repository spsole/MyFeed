using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
         
        public Interaction<Unit, bool> Copied { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
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

            Copied = new Interaction<Unit, bool>();
            Copy = ReactiveCommand.CreateFromTask(DoCopy,
                Observable.Return(!string.IsNullOrWhiteSpace(Url)));
        }

        private async Task DoCopy()
        {
            await _platformService.CopyTextToClipboard(Url);
            await Copied.Handle(Unit.Default);
        }
    }
}