﻿using System;
using System.Collections.ObjectModel;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SourcesCategoryViewModel
    {
        public SourcesCategoryViewModel(
            SourceCategoryEntity entity,
            SourcesViewModel parentViewModel,
            IDialogService dialogService,
            IPlatformService platformService,
            ISourcesRepository sourcesRepository,
            ITranslationsService translationsService)
        {
            Items = new ObservableCollection<SourcesItemViewModel>();
            SourceUri = ObservableProperty.Of(string.Empty);
            Category = ObservableProperty.Of(entity);
            Title = ObservableProperty.Of(entity.Title);
            RenameCategory = ActionCommand.Of(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                await sourcesRepository.RenameAsync(entity, name);
                entity.Title = Title.Value = name;
            });
            RemoveCategory = ActionCommand.Of(async () =>
            {
                var shouldDelete = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve("DeleteCategory"),
                    translationsService.Resolve("DeleteElement"));
                if (!shouldDelete) return;
                await sourcesRepository.RemoveAsync(entity);
                parentViewModel.Items.Remove(this);
            });
            AddSource = ActionCommand.Of(async () =>
            {
                var sourceUri = SourceUri.Value;
                if (string.IsNullOrWhiteSpace(sourceUri) ||
                    !Uri.IsWellFormedUriString(sourceUri, UriKind.Absolute))
                    return;
                SourceUri.Value = string.Empty;
                var model = new SourceEntity {Uri = sourceUri, Notify = true};
                await sourcesRepository.AddSourceAsync(entity, model);
                Items.Add(new SourcesItemViewModel(model, this,
                    sourcesRepository, platformService));
            });
            Load = ActionCommand.Of(() =>
            {
                Items.Clear();
                foreach (var source in entity.Sources)
                    Items.Add(new SourcesItemViewModel(source,
                        this, sourcesRepository, platformService));
            });
        }

        /// <summary>
        /// Inner items collection.
        /// </summary>
        public ObservableCollection<SourcesItemViewModel> Items { get; }

        /// <summary>
        /// Read-only category entity.
        /// </summary>
        public ObservableProperty<SourceCategoryEntity> Category { get; }

        /// <summary>
        /// Source Uri for new category user input.
        /// </summary>
        public ObservableProperty<string> SourceUri { get; }

        /// <summary>
        /// Grouping title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Removes the entire category.
        /// </summary>
        public ActionCommand RemoveCategory { get; }

        /// <summary>
        /// Renames the category.
        /// </summary>
        public ActionCommand RenameCategory { get; }

        /// <summary>
        /// Adds new source to this category.
        /// </summary>
        public ActionCommand AddSource { get; }

        /// <summary>
        /// Loads items.
        /// </summary>
        public ActionCommand Load { get; }
    }
}