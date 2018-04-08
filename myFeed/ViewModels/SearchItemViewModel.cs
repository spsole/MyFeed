using System;
using System.Collections.Generic;
using System.Linq;
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
    [ExportEx(typeof(SearchItemViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SearchItemViewModel
    {
        [DoNotCheckEquality]
        private FeedlyItem FeedlyItem { get; }

        public Interaction<IList<string>, int> AddSelect { get; }
        public ReactiveCommand Open { get; }
        public ReactiveCommand Copy { get; }
        public ReactiveCommand Add { get; }
        
        public string Description => FeedlyItem.Description;
        public string Image => FeedlyItem.IconUrl;
        public string Title => FeedlyItem.Title;
        public string Url => FeedlyItem.Website;
        
        public SearchItemViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            FeedlyItem feedlyItem)
        {
            FeedlyItem = feedlyItem;
            Open = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new Uri(Url)),
                this.WhenAnyValue(x => x.Url).Select(x => Uri
                    .IsWellFormedUriString(x, UriKind.Absolute))
            );
            Copy = ReactiveCommand.CreateFromTask(
                () => platformService.CopyTextToClipboard(Url),
                this.WhenAnyValue(x => x.Url).Select(x => 
                    !string.IsNullOrWhiteSpace(x))
            );
            AddSelect = new Interaction<IList<string>, int>();
            Add = ReactiveCommand.CreateFromTask(async () =>
            {
                var response = await categoryManager.GetAllAsync();
                var categories = new List<Category>(response);
                var titles = categories.Select(i => i.Title).ToList(); 
                var index = await AddSelect.Handle(titles);
                if (index < 0) return;

                var feed = FeedlyItem.FeedId?.Substring(5);
                var source = new Channel {Notify = true, Uri = feed}; 
                var category = categories[index]; 
                category.Channels.Add(source); 
                await categoryManager.UpdateAsync(category); 
            },
            this.WhenAnyValue(x => x.FeedlyItem)
                .Select(x => x.FeedId?.Substring(5))
                .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)));
        }
    }
}