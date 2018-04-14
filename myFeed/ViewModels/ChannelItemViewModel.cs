using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
        private Channel Channel { get; }
        
        public Interaction<Unit, bool> DeleteRequest { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }

        public string Name => new Uri(Channel.Uri).Host;
        public string Url => Channel.Uri;
        public bool Notify
        {
            get => Channel.Notify;
            set => Channel.Notify = value;
        }

        public ChannelItemViewModel(
            Category category, Channel channel,
            ICategoryManager categoryManager,
            IPlatformService platformService,
            IMessageBus messageBus)
        {
            Channel = channel;
            DeleteRequest = new Interaction<Unit, bool>();
            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await DeleteRequest.Handle(Unit.Default)) return;
                category.Channels.Remove(channel);
                await categoryManager.UpdateAsync(category);
                messageBus.SendMessage(this);
            });
            this.WhenAnyValue(x => x.Notify)
                .Select(x => categoryManager
                    .UpdateChannelAsync(channel)
                    .ToObservable())
                .Concat().Subscribe();

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