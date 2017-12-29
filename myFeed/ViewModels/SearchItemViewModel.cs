using System;
using System.ComponentModel.Composition;
using System.Linq;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(SearchItemViewModel))]
    public sealed class SearchItemViewModel
    {
        public ObservableProperty<string> Title { get; }
        public ObservableProperty<string> ImageUri { get; }
        public ObservableProperty<string> Description { get; }
        public ObservableProperty<string> Url { get; }

        public ObservableCommand AddToSources { get; }
        public ObservableCommand OpenInEdge { get; }
        public ObservableCommand CopyLink { get; }
        
        public SearchItemViewModel(
            ITranslationService translationService,
            ICategoryManager categoryManager,
            IPlatformService platformService,
            IStateContainer stateContainer,
            IDialogService dialogService)
        {
            var feedlyItem = stateContainer.Pop<FeedlyItem>();
            Description = feedlyItem.Description;
            ImageUri = feedlyItem.IconUrl;
            Url = feedlyItem.Website;
            Title = feedlyItem.Title;
            
            CopyLink = new ObservableCommand(() => platformService.CopyTextToClipboard(feedlyItem.Website));
            OpenInEdge = new ObservableCommand(async () =>
            {
                if (Uri.IsWellFormedUriString(feedlyItem.Website, UriKind.Absolute))
                    await platformService.LaunchUri(new Uri(feedlyItem.Website));
            });
            AddToSources = new ObservableCommand(async () =>
            {
                var feedUrl = feedlyItem.FeedId?.Substring(5); 
                if (!Uri.IsWellFormedUriString(feedUrl, UriKind.Absolute)) return; 

                var categories = (await categoryManager.GetAllAsync()).ToList();
                var titles = categories.Select(i => i.Title); 
                var title = translationService.Resolve("AddIntoCategory"); 
                var index = await dialogService.ShowDialogForSelection(title, titles); 
                if (index < 0) return; 

                var source = new Channel {Notify = true, Uri = feedUrl}; 
                var category = categories[index]; 
                category.Channels.Add(source); 
                await categoryManager.UpdateAsync(category); 
            });
        }
    }
}