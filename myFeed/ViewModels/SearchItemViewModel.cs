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
    [ExportEx(typeof(SearchItemViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SearchItemViewModel
    {
        public Interaction<IList<string>, int> AddSelect { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }
        
        public string Description { get; } 
        public string Image { get; } 
        public string Title { get; } 
        public string Url { get; } 
        
        public SearchItemViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            FeedlyItem feedlyItem)
        {
            Description = feedlyItem.Description;
            Image = feedlyItem.IconUrl;
            Title = feedlyItem.Title;
            Url = feedlyItem.Website;
            
            Open = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new Uri(Url)),
                Observable.Return(Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            );
            Copy = ReactiveCommand.CreateFromTask(
                () => platformService.CopyTextToClipboard(Url),
                Observable.Return(!string.IsNullOrWhiteSpace(Url))
            );
            
            AddSelect = new Interaction<IList<string>, int>();
            Add = ReactiveCommand.CreateFromTask(async () =>
            {
                var response = await categoryManager.GetAllAsync();
                var categories = new List<Category>(response);
                var titles = categories.Select(i => i.Title).ToList(); 
                var index = await AddSelect.Handle(titles);
                if (index < 0) return;

                var feed = feedlyItem.FeedId?.Substring(5);
                var source = new Channel {Notify = true, Uri = feed}; 
                var category = categories[index]; 
                category.Channels.Add(source); 
                await categoryManager.UpdateAsync(category); 
            },
            Observable.Return(feedlyItem.FeedId?.Substring(5)).Select(x => 
                Uri.IsWellFormedUriString(x, UriKind.Absolute)));
        }
    }
}