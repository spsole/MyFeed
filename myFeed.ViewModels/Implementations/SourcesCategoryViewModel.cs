using System;
using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    /// <summary>
    /// Sources category ViewModel.
    /// </summary>
    public sealed class SourcesCategoryViewModel
    {
        /// <summary>
        /// Instantiates new ViewModel.
        /// </summary>
        public SourcesCategoryViewModel(
            SourceCategoryEntity entity,
            SourcesViewModel parentViewModel,
            ISourcesRepository sourcesRepository,
            ITranslationsService translationsService,
            IPlatformProvider platformProvider)
        {
            Title = new ObservableProperty<string>(entity.Title);
            SourceUri = new ObservableProperty<string>(string.Empty);
            Items = new ObservableCollection<SourcesItemViewModel>();
            Category = new ReadOnlyProperty<SourceCategoryEntity>(entity);

            RenameCategory = new ActionCommand(async () =>
            {
                var name = await platformProvider.ShowDialogForResults(
                    translationsService.Resolve("EnterNameOfNewCategory"),
                    translationsService.Resolve("EnterNameOfNewCategoryTitle"));
                if (string.IsNullOrWhiteSpace(name)) return;
                await sourcesRepository.RenameAsync(entity, name);
                entity.Title = Title.Value = name;
            });
            RemoveCategory = new ActionCommand(async () =>
            {
                var shouldDelete = await platformProvider.ShowDialogForConfirmation(
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
                var viewModel = new SourcesItemViewModel(model,
                    this, sourcesRepository, platformProvider);
                Items.Add(viewModel);
            });
            Load = new ActionCommand(() =>
            {
                Items.Clear();
                foreach (var source in entity.Sources)
                {
                    var viewModel = new SourcesItemViewModel(source,
                        this, sourcesRepository, platformProvider);
                    Items.Add(viewModel);
                }
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