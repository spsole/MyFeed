using System;
using System.Linq;
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
            IGrouping<string, Article> parameter,
            IFactoryService factoryService)
        {
            var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
            Items = new ReactiveList<FeedItemViewModel>();
            Items.AddRange(parameter.Select(x => factory(x)));
            Title = parameter.Key;
        }
    }
}