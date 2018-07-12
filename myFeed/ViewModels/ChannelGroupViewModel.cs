using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(ChannelGroupViewModel))]
    public sealed class ChannelGroupViewModel
    {
        private readonly Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> _factory;
        private readonly ChannelViewModel _channelsViewModel;
        private readonly ICategoryManager _categoryManager;

        public ReactiveList<ChannelItemViewModel> Channels { get; }
        public ReactiveCommand<Unit, Channel> CreateChannel { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public Category Category { get; }

        public string ChannelUri { get; set; } = string.Empty;
        public string RealTitle { get; private set; }
        public string Title { get; set; }
        
        public ChannelGroupViewModel(
            Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> factory,
            ChannelViewModel channelsViewModel,
            ICategoryManager categoryManager,
            Category category)
        {
            _factory = factory;
            _categoryManager = categoryManager;
            _channelsViewModel = channelsViewModel;
            Category = category;

            Channels = new ReactiveList<ChannelItemViewModel>();
            Channels.AddRange(Category.Channels.Select(x => _factory(x, Category, this)));
            CreateChannel = ReactiveCommand.CreateFromTask(DoCreateChannel,
                this.WhenAnyValue(x => x.ChannelUri)
                    .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)));
            
            CreateChannel
                .Select(channel => string.Empty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ChannelUri = x);
            CreateChannel
                .Select(channel => _factory(channel, Category, this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Channels.Insert(0, x));
            
            Remove = ReactiveCommand.CreateFromTask(() => _categoryManager.Remove(Category));
            Remove.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => _channelsViewModel.Categories.Remove(this));
            
            Title = RealTitle = category.Title;
            this.ObservableForProperty(x => x.Title)
                .Select(property => property.Value)
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(title => title?.Trim())
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .DistinctUntilChanged()
                .Do(title => Category.Title = title)
                .Select(title => Category)
                .Select(_categoryManager.Update)
                .SelectMany(task => task.ToObservable())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => RealTitle = Category.Title);            
        }

        private async Task<Channel> DoCreateChannel()
        {
            var channel = new Channel {Uri = ChannelUri, Notify = true};
            Category.Channels.Insert(0, channel);
            await _categoryManager.Update(Category);
            return channel;
        }
    }
}