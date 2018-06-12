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
        private readonly Func<Channel, ChannelItemViewModel> _factory;
        private readonly ICategoryManager _categoryManager;
        private readonly IMessageBus _messageBus;
        private readonly Category _category;

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
            _categoryManager = categoryManager;
            _messageBus = messageBus;
            _category = category;
            _factory = factory;

            Title = category.Title;
            Items = new ReactiveList<ChannelItemViewModel>();
            messageBus.Listen<ChannelDeleteEvent>()
                .Do(x => _category.Channels.Remove(x.Channel))
                .Do(x => Items.Remove(x.ChannelItemViewModel))
                .SelectMany(x => _categoryManager.Update(category))
                .Subscribe();

            Load = ReactiveCommand.Create(
                () => Items.AddRange(_category.Channels.Select(factory))
            );
            AddChannel = ReactiveCommand.CreateFromTask(DoAddChannel,
                this.WhenAnyValue(x => x.ChannelUri, x => Uri
                    .IsWellFormedUriString(x, UriKind.Absolute)));

            RemoveRequest = new Interaction<Unit, bool>();
            RenameRequest = new Interaction<Unit, string>();
            Remove = ReactiveCommand.CreateFromTask(DoRemove);
            Rename = ReactiveCommand.CreateFromTask(DoRename);
        }

        private async Task DoAddChannel()
        {
            var uri = ChannelUri;
            ChannelUri = string.Empty;
            var model = new Channel { Uri = uri, Notify = true };
            _category.Channels.Add(model);
            await _categoryManager.Update(_category); 
            Items.Add(_factory(model));   
        }

        private async Task DoRemove() 
        {
            if (!await RemoveRequest.Handle(Unit.Default)) return;
            _messageBus.SendMessage(new CategoryDeleteEvent(_category, this));
        }

        private async Task DoRename() 
        {
            var name = await RenameRequest.Handle(Unit.Default);
            if (string.IsNullOrWhiteSpace(name)) return;
            Title = _category.Title = name;
            await _categoryManager.Update(_category);
        }
    }
}