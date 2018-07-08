using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(ChannelItemViewModel))]
    public sealed class ChannelItemViewModel
    {
        private readonly ChannelGroupViewModel _channelGroup;
        private readonly ICategoryManager _categoryManager;
        private readonly IPlatformService _platformService;
        private readonly Category _category;
        private readonly Channel _channel;

        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public Interaction<Unit, bool> Copied { get; }

        public string Name => new Uri(_channel.Uri).Host.ToUpperInvariant();
        public string Url => _channel.Uri;
        public bool Notify { get; set; }

        public ChannelItemViewModel(
            ChannelGroupViewModel channelGroup,
            ICategoryManager categoryManager,
            IPlatformService platformService,
            Category category,
            Channel channel)
        {
            _categoryManager = categoryManager;
            _platformService = platformService;
            _channelGroup = channelGroup;
            _category = category;
            _channel = channel;

            Open = ReactiveCommand.CreateFromTask(DoOpen,
                this.WhenAnyValue(x => x.Url, url => Uri
                    .IsWellFormedUriString(url, UriKind.Absolute)));
            
            Notify = _channel.Notify;
            this.ObservableForProperty(x => x.Notify)
                .Select(property => property.Value)
                .Do(notify => _channel.Notify = notify)
                .Select(notify => channel)
                .Select(_categoryManager.Update)
                .SelectMany(task => task.ToObservable())
                .Subscribe();
            
            Delete = ReactiveCommand.CreateFromTask(DoDelete);
            Delete.ObserveOn(RxApp.MainThreadScheduler)
                  .Subscribe(x => _channelGroup.Channels.Remove(this));

            Copied = new Interaction<Unit, bool>();
            Copy = ReactiveCommand.CreateFromTask(() => 
                _platformService.CopyTextToClipboard(Url));
            Copy.ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(Copied.Handle)
                .Subscribe();
        }

        private async Task DoDelete() 
        {
            _category.Channels.Remove(_channel);
            await _categoryManager.Update(_category);
        }

        private async Task DoOpen()
        {
            var url = new Uri(_channel.Uri);
            var site = $"http://{url.Host}";
            await _platformService.LaunchUri(new Uri(site));
        }
    }
}