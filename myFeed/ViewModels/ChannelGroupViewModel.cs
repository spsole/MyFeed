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
        
        public ReactiveCommand AddChannel { get; }
        public ReactiveCommand Remove { get; }
        public ReactiveCommand Rename { get; }
        public ReactiveCommand Load { get; }

        public string ChannelUri { get; set; }
        public string Title { get; private set; }
        
        public ChannelGroupViewModel(
            Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> factory,
            ChannelViewModel channelViewModel,
            ICategoryManager categoryManager,
            Category category)
        {
            Title = category.Title;
            ChannelUri = string.Empty;
            Items = new ReactiveList<ChannelItemViewModel>();

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
                await categoryManager.RemoveAsync(category);
                channelViewModel.Items.Remove(this);
            });
            
            AddChannel = ReactiveCommand.CreateFromTask(async () =>
            {
                var model = new Channel {Uri = ChannelUri, Notify = true};
                ChannelUri = string.Empty;
                category.Channels.Add(model);
                await categoryManager.UpdateAsync(category);
                Items.Add(factory(model, category, this));
            }, 
            this.WhenAnyValue(x => x.ChannelUri).Select(x => Uri
                .IsWellFormedUriString(x, UriKind.Absolute)));
            
            Load = ReactiveCommand.Create(() =>
            {
                Items.Clear();
                Items.AddRange(category.Channels
                     .Select(x => factory(x, category, this)));
            });
        }
    }
}