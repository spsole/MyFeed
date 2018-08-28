using System;
using System.Linq;
using DryIocAttributes;
using MyFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(FaveGroupViewModel))]
    public sealed class FaveGroupViewModel
    {
        private readonly Func<Article, FeedItemViewModel> _factory;
        private readonly IReactiveList<FeedItemViewModel> _source;
        private readonly IGrouping<string, Article> _grouping;

        public IReactiveDerivedList<FeedItemViewModel> Items { get; }
        public string Title => _grouping.Key;

        public FaveGroupViewModel(
            Func<Article, FeedItemViewModel> factory,
            IGrouping<string, Article> grouping)
        {
            _factory = factory;
            _grouping = grouping;
            _source = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            Items = _source.CreateDerivedCollection(x => x, x => x.Fave);
            _source.AddRange(_grouping.Select(_factory));
        }
    }
}