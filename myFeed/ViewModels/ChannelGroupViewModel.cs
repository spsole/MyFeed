using System;
using System.Linq;
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
    [ExportEx(typeof(ChannelGroupViewModel))]
    public sealed class ChannelGroupViewModel
    {
        public ReactiveList<ChannelItemViewModel> Items { get; }
        public ReactiveCommand AddChannel { get; }
        public ReactiveCommand Remove { get; }
        public ReactiveCommand Rename { get; }
        public ReactiveCommand Load { get; }

        public string ChannelUri { get; set; }
        public string Title { get; private set; }

        public ChannelGroupViewModel(
            ITranslationService translationsService,
            ChannelViewModel channelsViewModel,
            ICategoryManager categoryManager,
            IFactoryService factoryService,
            IDialogService dialogService,
            Category category)
        {
            (Title, ChannelUri) = (category.Title, string.Empty);
            Items = new ReactiveList<ChannelItemViewModel>();
            var factory = factoryService.Create<Func<Channel, Category, 
                ChannelGroupViewModel, ChannelItemViewModel>>();
            
            Rename = ReactiveCommand.CreateFromTask(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve(Constants.EnterNameOfNewCategory),
                    translationsService.Resolve(Constants.EnterNameOfNewCategoryTitle));
                if (string.IsNullOrWhiteSpace(name)) return;
                Title = category.Title = name;
                await categoryManager.UpdateAsync(category);
            });
            Remove = ReactiveCommand.CreateFromTask(async () =>
            {
                var shouldDelete = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve(Constants.DeleteCategory),
                    translationsService.Resolve(Constants.DeleteElement));
                if (!shouldDelete) return;
                await categoryManager.RemoveAsync(category);
                channelsViewModel.Items.Remove(this);
            });
            AddChannel = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var model = new Channel {Uri = ChannelUri, Notify = true};
                    ChannelUri = string.Empty;
                    category.Channels.Add(model);
                    await categoryManager.UpdateAsync(category);
                    Items.Add(factory(model, category, this));
                }, 
                this.WhenAnyValue(x => x.ChannelUri)
                    .Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
            );
            Load = ReactiveCommand.Create(() =>
            {
                Items.Clear();
                var viewModels = category.Channels.Select(x => factory(x, category, this));
                Items.AddRange(viewModels);
            });
        }
    }
}