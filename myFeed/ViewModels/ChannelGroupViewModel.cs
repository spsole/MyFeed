using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
                .Where(x => category.Channels.Contains(x.Model))
                .Do(x => category.Channels.Remove(x.Model))
                .Where(x => Items.Contains(x.ViewModel))
                .Do(x => Items.Remove(x.ViewModel))
                .SelectMany(x => categoryManager
                    .UpdateAsync(category)
                    .ToObservable())
                .Subscribe();

            RenameRequest = new Interaction<Unit, string>();
            Rename = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await RenameRequest.Handle(Unit.Default);
                if (string.IsNullOrWhiteSpace(name)) return;
                Title = category.Title = name;
                await categoryManager.UpdateAsync(category);
            });

            RemoveRequest = new Interaction<Unit, bool>();
            Remove = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await RemoveRequest.Handle(Unit.Default)) return;
                messageBus.SendMessage(new CategoryDeleteEvent(category, this));
            });

            AddChannel = ReactiveCommand.CreateFromTask(async () =>
            {
                var model = new Channel {Uri = ChannelUri, Notify = true};
                ChannelUri = string.Empty;
                category.Channels.Add(model);
                await categoryManager.UpdateAsync(category);
                Items.Add(factory(model));
            }, 
            this.WhenAnyValue(x => x.ChannelUri)
            	.Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)));
            
            Load = ReactiveCommand.Create(() =>
            {
                Items.Clear();
                var items = category.Channels.Select(factory);
                Items.AddRange(items);
            });
        }
    }
}