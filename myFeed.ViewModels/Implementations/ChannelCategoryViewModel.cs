using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(ChannelCategoryViewModel))]
    public sealed class ChannelCategoryViewModel
    {
        public ObservableCollection<ChannelViewModel> Items { get; }

        public ObservableProperty<string> SourceUri { get; }
        public ObservableProperty<string> Title { get; }

        public ObservableCommand RemoveCategory { get; }
        public ObservableCommand RenameCategory { get; }
        public ObservableCommand AddSource { get; }
        public ObservableCommand Load { get; }

        public ChannelCategoryViewModel(
            ITranslationsService translationsService,
            ICategoryManager categoryManager,
            IStateContainer stateContainer,
            IFactoryService factoryService,
            IDialogService dialogService)
        {
            var category = stateContainer.Pop<Category>();
            var channelsViewModel = stateContainer.Pop<ChannelsViewModel>();
            (Title, SourceUri) = (category.Title, string.Empty);
            
            Items = new ObservableCollection<ChannelViewModel>();
            RenameCategory = new ObservableCommand(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                category.Title = name;
                await categoryManager.UpdateAsync(category);
                category.Title = Title.Value = name;
            });
            RemoveCategory = new ObservableCommand(async () =>
            {
                var shouldDelete = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve("DeleteCategory"),
                    translationsService.Resolve("DeleteElement"));
                if (!shouldDelete) return;
                await categoryManager.RemoveAsync(category);
                channelsViewModel.Items.Remove(this);
            });
            AddSource = new ObservableCommand(async () =>
            {
                var sourceUri = SourceUri.Value;
                if (string.IsNullOrWhiteSpace(sourceUri) || !Uri
                    .IsWellFormedUriString(sourceUri, UriKind.Absolute)) return;
                SourceUri.Value = string.Empty;
                var model = new Channel {Uri = sourceUri, Notify = true};
                category.Channels.Add(model);
                await categoryManager.UpdateAsync(category);
                Items.Add(factoryService.CreateInstance<ChannelViewModel>(model, this));
            });
            Load = new ObservableCommand(() =>
            {
                Items.Clear();
                foreach (var channel in category.Channels) 
                    Items.Add(factoryService.CreateInstance<
                        ChannelViewModel>(channel, this, category));
            });
        }
    }
}