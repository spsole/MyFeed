using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DryIocAttributes;
using myFeed.Interfaces;
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
        private readonly ICategoryManager _categoryManager;
        private readonly IPlatformService _platformService;
        private readonly FeedlyItem _feedlyItem;

        public Interaction<IList<string>, int> Select { get; }
        public Interaction<Exception, bool> Error { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public string Description => _feedlyItem.Description;
        public string Image => _feedlyItem.IconUrl;
        public string Title => _feedlyItem.Title;
        public string Url => _feedlyItem.Website;
        
        public SearchItemViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            FeedlyItem feedlyItem)
        {
            _categoryManager = categoryManager;
            _platformService = platformService;
            _feedlyItem = feedlyItem;

            Select = new Interaction<IList<string>, int>();
            Add = ReactiveCommand.CreateFromTask(DoAdd, Observable
                .Return(feedlyItem.FeedId?.Substring(5))
                .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)));

            Error = new Interaction<Exception, bool>();
            Add.ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(error => Error.Handle(error));

            Open = ReactiveCommand.CreateFromTask(
                () => _platformService.LaunchUri(new Uri(Url)),
                Observable.Return(Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            );
            Copy = ReactiveCommand.CreateFromTask(
                () => _platformService.CopyTextToClipboard(Url),
                Observable.Return(!string.IsNullOrWhiteSpace(Url))
            );
        }

        private async Task DoAdd()
        {
            var response = await _categoryManager.GetAll();
            var categories = new List<Category>(response);
            var titles = categories.Select(i => i.Title).ToList(); 
            var index = await Select.Handle(titles);
            if (index < 0) return;

            var feed = _feedlyItem.FeedId?.Substring(5);
            var category = categories[index];

            var source = new Channel { Notify = true, Uri = feed };
            category.Channels.Add(source); 
            await _categoryManager.Update(category); 
        }
    }
}