using System;
using System.Linq;
using DryIocAttributes;
using myFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(FaveGroupViewModel))]
    public sealed class FaveGroupViewModel
    {
        public IReactiveDerivedList<FeedItemViewModel> Items { get; }
        public string Title { get; }

        public FaveGroupViewModel(
            Func<Article, FeedItemViewModel> factory,
            IGrouping<string, Article> grouping)
        {
            Title = grouping.Key;
            var cache = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            Items = cache.CreateDerivedCollection(x => x, x => x.Fave);
            cache.AddRange(grouping.Select(factory));
        }
    }
}