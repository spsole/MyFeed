using System;
using System.Linq;
using DryIoc;
using DryIocAttributes;
using myFeed.Interfaces;
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
            IGrouping<string, Article> grouping,
            IResolver resolver)
        {
            Title = grouping.Key;
            var factory = resolver.Resolve<Func<Article, FeedItemViewModel>>();
            var cache = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            Items = cache.CreateDerivedCollection(x => x, x => x.Fave);
            cache.AddRange(grouping.Select(x => factory(x)));
        }
    }
}