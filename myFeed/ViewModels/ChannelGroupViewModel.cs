using System;
using System.Linq;
using System.Reactive;
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
            Func<Channel, Category, ChannelItemViewModel> factory,
            ICategoryManager categoryManager,
            IMessageBus messageBus,
            Category category)
        {
            Title = category.Title;
            Items = new ReactiveList<ChannelItemViewModel>();
            RenameRequest = new Interaction<Unit, string>();
            RemoveRequest = new Interaction<Unit, bool>();
            messageBus.Listen<ChannelItemViewModel>()
                      .Subscribe(x => Items.Remove(x));

            Rename = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await RenameRequest.Handle(Unit.Default);
                if (string.IsNullOrWhiteSpace(name)) return;
                Title = category.Title = name;
                await categoryManager.UpdateAsync(category);
            });
            Remove = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await RemoveRequest.Handle(Unit.Default)) return;
                await categoryManager.RemoveAsync(category);
                messageBus.SendMessage(this);
            });
            AddChannel = ReactiveCommand.CreateFromTask(async () =>
            {
                var model = new Channel {Uri = ChannelUri, Notify = true};
                ChannelUri = string.Empty;
                category.Channels.Add(model);
                await categoryManager.UpdateAsync(category);
                Items.Add(factory(model, category));
            }, 
            this.WhenAnyValue(x => x.ChannelUri).Select(x => Uri
                .IsWellFormedUriString(x, UriKind.Absolute)));
            
            Load = ReactiveCommand.Create(() =>
            {
                Items.Clear();
                Items.AddRange(category.Channels
                     .Select(x => factory(x, category)));
            });
        }
    }
}