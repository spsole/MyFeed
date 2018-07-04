using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIoc;
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

        public ReactiveList<ChannelItemViewModel> Channels { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }

        public string ChannelUri { get; set; } = string.Empty;
        public string RealTitle { get; private set; }
        public string Title { get; set; }
        
        public ChannelGroupViewModel(
            Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> factory,
            ChannelViewModel channelsViewModel,
            ICategoryManager categoryManager,
            Category category)
        {
            _channelsViewModel = channelsViewModel;
            _categoryManager = categoryManager;
            _category = category;
            _factory = factory;

            Title = RealTitle = category.Title;
            Channels = new ReactiveList<ChannelItemViewModel>();
            Channels.AddRange(_category.Channels.Select(x => _factory(x, _category, this)));
            Add = ReactiveCommand.CreateFromTask(DoAdd,
                this.WhenAnyValue(x => x.ChannelUri)
                    .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)));
            
            Remove = ReactiveCommand.CreateFromTask(DoRemove);   
            this.WhenAnyValue(x => x.Title).Skip(1)
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(title => title?.Trim())
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(title => RealTitle = _category.Title = title)
                .Select(title => _category)
                .SelectMany(_categoryManager.Update)
                .Subscribe();            
        }

        private async Task DoAdd()
        {
            var uri = ChannelUri;
            ChannelUri = string.Empty;
            var model = new Channel { Uri = uri, Notify = true };
            _category.Channels.Add(model);
            await _categoryManager.Update(_category); 
            Channels.Add(_factory(model, _category, this));   
        }

        private async Task DoRemove() 
        {
            await _categoryManager.Remove(_category);
            _channelsViewModel.Categories.Remove(this);
        }
    }
}