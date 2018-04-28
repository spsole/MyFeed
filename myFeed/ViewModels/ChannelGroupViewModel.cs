using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Events;
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
        public ReactiveList<ChannelItemViewModel> Items { get; }
        public Interaction<Unit, string> RenameRequest { get; }  
        public Interaction<Unit, bool> RemoveRequest { get; }
        
        public ReactiveCommand<Unit, Unit> AddChannel { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Rename { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }

        public string ChannelUri { get; set; } = string.Empty;
        public string Title { get; private set; }
        
        public ChannelGroupViewModel(
            Func<Channel, ChannelItemViewModel> factory,
            ICategoryManager categoryManager, 
            IMessageBus messageBus,
            Category category)
        {
            Title = category.Title;
            Items = new ReactiveList<ChannelItemViewModel>();
            messageBus.Listen<ChannelDeleteEvent>()
                .Where(x => category.Channels.Contains(x.Channel))
                .Do(x => category.Channels.Remove(x.Channel))
                .Where(x => Items.Contains(x.ChannelItemViewModel))
                .Do(x => Items.Remove(x.ChannelItemViewModel))
                .SelectMany(x => categoryManager.Update(category))
                .Subscribe();

            Load = ReactiveCommand.Create(
                () => Items.AddRange(category.Channels.Select(factory))
            );
            AddChannel = ReactiveCommand.CreateFromTask(
                () => DoAddChannel(factory, categoryManager, category),
                this.WhenAnyValue(x => x.ChannelUri, x => Uri
                    .IsWellFormedUriString(x, UriKind.Absolute))
            );

            RemoveRequest = new Interaction<Unit, bool>();
            Remove = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await RemoveRequest.Handle(Unit.Default)) return;
                messageBus.SendMessage(new CategoryDeleteEvent(category, this));
            });

            RenameRequest = new Interaction<Unit, string>();
            Rename = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await RenameRequest.Handle(Unit.Default);
                if (string.IsNullOrWhiteSpace(name)) return;
                Title = category.Title = name;
                await categoryManager.Update(category);
            });
        }

        private async Task DoAddChannel(
            Func<Channel, ChannelItemViewModel> factory,
            ICategoryManager categoryManager,
            Category category)
        {
            var uri = ChannelUri;
            ChannelUri = string.Empty;
            var model = new Channel { Uri = uri, Notify = true };
            
            category.Channels.Add(model);
            await categoryManager.Update(category); 
            Items.Add(factory(model));   
        }
    }
}