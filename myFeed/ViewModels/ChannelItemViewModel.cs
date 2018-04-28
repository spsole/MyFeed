using System;
using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using myFeed.Events;
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
        public Interaction<Unit, bool> DeleteRequest { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }

        public bool Notify { get; set; }
        public string Name { get; }
        public string Url { get; }

        public ChannelItemViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            IMessageBus messageBus,
            Channel channel)
        {
            Url = channel.Uri;
            Notify = channel.Notify;
            Name = new Uri(channel.Uri).Host;
            this.WhenAnyValue(x => x.Notify)
                .Skip(1).Do(x => channel.Notify = x)
                .SelectMany(x => categoryManager.Update(channel))
                .Subscribe();
            
            DeleteRequest = new Interaction<Unit, bool>();
            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await DeleteRequest.Handle(Unit.Default)) return;
                messageBus.SendMessage(new ChannelDeleteEvent(channel, this));
            });

            Copy = ReactiveCommand.CreateFromTask(() => platformService.CopyTextToClipboard(Url));
            Open = ReactiveCommand.CreateFromTask(async () =>
            {
                var url = new Uri(Url);
                var builder = new UriBuilder(url) {Fragment = string.Empty};
                await platformService.LaunchUri(builder.Uri);
            },
            this.WhenAnyValue(x => x.Url).Select(x => Uri
                .IsWellFormedUriString(x, UriKind.Absolute)));
        }
    }
}