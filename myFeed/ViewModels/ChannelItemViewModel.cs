using System;
using System.Reactive;
using System.Reactive.Linq;
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
        [DoNotCheckEquality]
        private Channel Channel { get; set; }
        
        public Interaction<Unit, bool> DeleteRequest { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }

        public string Name => new Uri(Channel.Uri).Host;
        public string Url => Channel.Uri;
        public bool Notify { get; set; } 

        public ChannelItemViewModel(
            Category category, Channel channel,
            ICategoryManager categoryManager,
            IPlatformService platformService,
            IMessageBus messageBus)
        {
            Channel = channel;
            Notify = channel.Notify;
            DeleteRequest = new Interaction<Unit, bool>();
            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await DeleteRequest.Handle(Unit.Default)) return;
                messageBus.SendMessage(this);
                category.Channels.Remove(channel);
                await categoryManager.UpdateAsync(category);
            });
            
            Copy = ReactiveCommand.CreateFromTask(() => platformService.CopyTextToClipboard(Url));
            Open = ReactiveCommand.CreateFromTask(async () =>
            {
                var builder = new UriBuilder(new Uri(Url)) {Fragment = string.Empty};
                await platformService.LaunchUri(builder.Uri);
            },
            this.WhenAnyValue(x => x.Url).Select(x => Uri
                .IsWellFormedUriString(x, UriKind.Absolute)));
            
            this.WhenAnyValue(x => x.Notify)
                .Select(x => channel.Notify = x)
                .Subscribe(async x => await categoryManager
                    .UpdateChannelAsync(channel));
        }
    }
}