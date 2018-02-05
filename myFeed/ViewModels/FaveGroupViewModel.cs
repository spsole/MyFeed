using System;
using System.Linq;
using System.Reactive.Linq;
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
        public ReactiveList<FeedItemViewModel> Items { get; }
        public string Title { get; }

        public FaveGroupViewModel(
            IGrouping<string, Article> grouping,
            IFactoryService factoryService)
        {
            Items = new ReactiveList<FeedItemViewModel>();
            var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
            Items.AddRange(grouping.Select(x => factory(x)));
            Title = grouping.Key;
            foreach (var item in Items)
            {
                var index = Items.IndexOf(item);
                item.WhenAnyValue(x => x.Fave).Subscribe(fave =>
                {
                    var contains = Items.Contains(item);
                    if (fave && !contains) Items.Insert(index, item);
                    else if (!fave && contains) Items.Remove(item);
                });
            }
        }
    }
}