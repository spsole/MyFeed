using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
        private readonly Category _category;

        public ReactiveList<ChannelItemViewModel> Items { get; }
        public Interaction<Unit, string> RenameRequest { get; } 
        public Interaction<Unit, bool> RemoveRequest { get; }
        
        public ReactiveCommand<Unit, Unit> AddChannel { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; } 
        public ReactiveCommand<Unit, Unit> Rename { get; }

        public string ChannelUri { get; set; } = string.Empty;
        public string Title { get; private set; }
        
        public ChannelGroupViewModel(
            Func<Channel, Category, 
                ChannelGroupViewModel, 
                ChannelItemViewModel> factory,
            ChannelViewModel channelsViewModel,
            ICategoryManager categoryManager,
            Category category)
        {
            Items = new ReactiveList<ChannelItemViewModel>();
            RenameRequest = new Interaction<Unit, string>();
            RemoveRequest = new Interaction<Unit, bool>();
            _channelsViewModel = channelsViewModel;
            _categoryManager = categoryManager;
            _category = category;
            _factory = factory;

            Title = category.Title;
            var channels = _category.Channels;
            var models = channels.Select(x => _factory(x, _category, this));
            Items.AddRange(models);
            
            Remove = ReactiveCommand.CreateFromTask(DoRemove);
            Rename = ReactiveCommand.CreateFromTask(DoRename);
            AddChannel = ReactiveCommand.CreateFromTask(DoAddChannel,
                this.WhenAnyValue(x => x.ChannelUri, x => Uri
                    .IsWellFormedUriString(x, UriKind.Absolute)));
        }

        private async Task DoAddChannel()
        {
            var uri = ChannelUri;
            ChannelUri = string.Empty;
            var model = new Channel { Uri = uri, Notify = true };
            _category.Channels.Add(model);
            await _categoryManager.Update(_category); 
            Items.Add(_factory(model, _category, this));   
        }

        private async Task DoRemove() 
        {
            if (!await RemoveRequest.Handle(Unit.Default)) return;
            await _categoryManager.Remove(_category);
            _channelsViewModel.Items.Remove(this);
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