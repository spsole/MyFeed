using System;
using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SourcesCategoryViewModel
    {
        public SourcesCategoryViewModel(
            SourceCategoryEntity entity,
            SourcesViewModel parentViewModel,
            IPlatformService platformService,
            ISourcesRepository sourcesRepository,
            ITranslationsService translationsService)
        {
            Category = new ReadOnlyProperty<SourceCategoryEntity>(entity);
            SourceUri = new ObservableProperty<string>(string.Empty);
            Items = new ObservableCollection<SourcesItemViewModel>();
            Title = new ObservableProperty<string>(entity.Title);
            RenameCategory = new ActionCommand(async () =>
            {
                var name = await platformService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                await sourcesRepository.RenameAsync(entity, name);
                entity.Title = Title.Value = name;
            });
            RemoveCategory = new ActionCommand(async () =>
            {
                var shouldDelete = await platformService.ShowDialogForConfirmation(
                    translationsService.Resolve("DeleteCategory"),
                    translationsService.Resolve("DeleteElement"));
                if (!shouldDelete) return;
                await sourcesRepository.RemoveAsync(entity);
                parentViewModel.Items.Remove(this);
            });
            AddSource = new ActionCommand(async () =>
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
            Load = new ActionCommand(() =>
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
        public ReadOnlyProperty<SourceCategoryEntity> Category { get; }

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