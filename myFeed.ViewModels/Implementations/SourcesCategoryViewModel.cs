using System;
using System.Linq;
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
            Category = new Property<SourceCategoryEntity>(entity);
            SourceUri = new Property<string>(string.Empty);
            Items = new Collection<SourcesItemViewModel>();
            Title = new Property<string>(entity.Title);
            RenameCategory = new Command(async () =>
            {
                var name = await dialogService.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                await sourcesRepository.RenameAsync(entity, name);
                entity.Title = Title.Value = name;
            });
            RemoveCategory = new Command(async () =>
            {
                var shouldDelete = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve("DeleteCategory"),
                    translationsService.Resolve("DeleteElement"));
                if (!shouldDelete) return;
                await sourcesRepository.RemoveAsync(entity);
                parentViewModel.Items.Remove(this);
            });
            AddSource = new Command(async () =>
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
            Load = new Command(() =>
            {
                var sourcesViewModels = entity.Sources
                    .Select(i => new SourcesItemViewModel(i,
                         this, sourcesRepository, platformService));
                Items.Clear();
                Items.AddRange(sourcesViewModels);
            });
        }

        /// <summary>
        /// Inner items collection.
        /// </summary>
        public Collection<SourcesItemViewModel> Items { get; }

        /// <summary>
        /// Read-only category entity.
        /// </summary>
        public Property<SourceCategoryEntity> Category { get; }

        /// <summary>
        /// Source Uri for new category user input.
        /// </summary>
        public Property<string> SourceUri { get; }

        /// <summary>
        /// Grouping title.
        /// </summary>
        public Property<string> Title { get; }

        /// <summary>
        /// Removes the entire category.
        /// </summary>
        public Command RemoveCategory { get; }

        /// <summary>
        /// Renames the category.
        /// </summary>
        public Command RenameCategory { get; }

        /// <summary>
        /// Adds new source to this category.
        /// </summary>
        public Command AddSource { get; }

        /// <summary>
        /// Loads items.
        /// </summary>
        public Command Load { get; }
    }
}