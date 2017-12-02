using System;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(SearchItemViewModel))]
    public sealed class SearchItemViewModel
    {
        public ObservableProperty<string> Title { get; }
        public ObservableProperty<string> ImageUri { get; }
        public ObservableProperty<string> Description { get; }
        public ObservableProperty<string> FeedUrl { get; }
        public ObservableProperty<string> Url { get; }

        public ObservableCommand AddToSources { get; }
        public ObservableCommand OpenInEdge { get; }
        public ObservableCommand CopyLink { get; }
        
        public SearchItemViewModel(
            ICategoryStoreService categoriesRepository,
            IPlatformService platformService,
            IDialogService dialogService,
            FeedlyItem feedlyItem)
        {
            Url = feedlyItem.Website;
            Title = feedlyItem.Title;
            ImageUri = feedlyItem.IconUrl;
            Description = feedlyItem.Description;
            FeedUrl = feedlyItem.FeedId?.Substring(5);
            
            CopyLink = new ObservableCommand(() => platformService.CopyTextToClipboard(feedlyItem.Website));
            OpenInEdge = new ObservableCommand(async () =>
            {
                if (Uri.IsWellFormedUriString(feedlyItem.Website, UriKind.Absolute))
                    await platformService.LaunchUri(new Uri(feedlyItem.Website));
            });
            AddToSources = new ObservableCommand(async () =>
            {
                if (!Uri.IsWellFormedUriString(FeedUrl.Value, UriKind.Absolute)) return;
                var categories = await categoriesRepository.GetAllAsync();
                var response = await dialogService.ShowDialogForSelection(categories);
                if (response is Category category)
                {
                    var source = new Channel {Notify = true, Uri = FeedUrl.Value};
                    await categoriesRepository.InsertChannelAsync(category, source);
                }
            });
        }
    }
}