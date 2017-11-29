using System;
using System.Collections.ObjectModel;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ChannelCategoryViewModel
    {
        public ObservableCollection<ChannelViewModel> Items { get; }

        public ObservableProperty<Category> Category { get; }
        public ObservableProperty<string> SourceUri { get; }
        public ObservableProperty<string> Title { get; }

        public ObservableCommand RemoveCategory { get; }
        public ObservableCommand RenameCategory { get; }
        public ObservableCommand AddSource { get; }
        public ObservableCommand Load { get; }

        public ChannelCategoryViewModel(
            ICategoriesRepository categoriesRepository,
            ITranslationsService translationsService,
            IFactoryService factoryService,
            IDialogService dialogService,
            ChannelsViewModel channelsViewModel,
            Category category)
        {
            Category = category;
            Title = category.Title;
            SourceUri = string.Empty;
            
            Items = new ObservableCollection<ChannelViewModel>();
            RenameCategory = new ObservableCommand(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                category.Title = name;
                await categoriesRepository.UpdateAsync(category);
                category.Title = Title.Value = name;
            });
            RemoveCategory = new ObservableCommand(async () =>
            {
                var shouldDelete = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve("DeleteCategory"),
                    translationsService.Resolve("DeleteElement"));
                if (!shouldDelete) return;
                await categoriesRepository.RemoveAsync(category);
                channelsViewModel.Items.Remove(this);
            });
            AddSource = new ObservableCommand(async () =>
            {
                var sourceUri = SourceUri.Value;
                if (string.IsNullOrWhiteSpace(sourceUri) ||
                    !Uri.IsWellFormedUriString(sourceUri, UriKind.Absolute))
                    return;
                SourceUri.Value = string.Empty;
                var model = new Channel {Uri = sourceUri, Notify = true};
                await categoriesRepository.InsertChannelAsync(category, model);
                Items.Add(factoryService.CreateInstance<
                    ChannelViewModel>(model, this));
            });
            Load = new ObservableCommand(() =>
            {
                Items.Clear();
                foreach (var channel in category.Channels) 
                    Items.Add(factoryService.CreateInstance<
                        ChannelViewModel>(channel, this));
            });
        }
    }
}