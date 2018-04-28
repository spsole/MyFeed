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
        public Interaction<IList<string>, int> Select { get; }
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
            
            Select = new Interaction<IList<string>, int>();
            Add = ReactiveCommand.CreateFromTask(
                () => DoAdd(categoryManager, feedlyItem), Observable
                    .Return(feedlyItem.FeedId?.Substring(5))
                    .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
            );
            
            Open = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new Uri(Url)),
                Observable.Return(Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            );
            Copy = ReactiveCommand.CreateFromTask(
                () => platformService.CopyTextToClipboard(Url),
                Observable.Return(!string.IsNullOrWhiteSpace(Url))
            );
        }

        private async Task DoAdd(
            ICategoryManager categoryManager,
            FeedlyItem feedlyItem)
        {
            var response = await categoryManager.GetAll();
            var categories = new List<Category>(response);
            var titles = categories.Select(i => i.Title).ToList(); 
            var index = await Select.Handle(titles);
            if (index < 0) return;

            var feed = feedlyItem.FeedId?.Substring(5);
            var source = new Channel { Notify = true, Uri = feed }; 
            var category = categories[index]; 
            category.Channels.Add(source); 
            await categoryManager.Update(category); 
        }
    }
}