using Windows.ApplicationModel.Resources;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using MyFeed.ViewModels;
using ReactiveUI;
using System;

namespace MyFeed.Uwp.Views
{
    public sealed partial class SearchView : Page, IViewFor<SearchViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(SearchViewModel),
                typeof(SearchView), new PropertyMetadata(null));

        public SearchView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as SearchViewModel;
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel,
                    viewModel => viewModel.ViewCategories,
                    view => view.ViewCategoriesButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.RefreshButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                     viewModel => viewModel.Search,
                     view => view.SearchButton)
                     .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SearchQuery,
                    view => view.SearchQueryTextBox.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Categories,
                    view => view.CategoriesComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                    viewModel => viewModel.SelectedCategory,
                    view => view.CategoriesComboBox.SelectedItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Add,
                    view => view.AddButton)
                    .DisposeWith(disposables);
                ViewModel.Added
                    .RegisterHandler(async x => x.SetOutput(await HandleAdded()))
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Feeds,
                    view => view.FoundFeedsListView.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                    viewModel => viewModel.SelectedFeed,
                    view => view.FoundFeedsListView.SelectedItem)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.FoundFeedsListView.RefreshCommand)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsEmpty,
                    view => view.EmptyScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.EmptyScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsGreeting,
                    view => view.GreetingScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.GreetingScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.HasErrors,
                    view => view.HasErrorsScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.HasErrorsScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.LoadingScreen.IsActive)
                    .DisposeWith(disposables);
            });
        }

        private async Task<bool> HandleAdded()
        {
            var resources = ResourceLoader.GetForCurrentView();
            var dialog = new MessageDialog(
                resources.GetString("SearchAddedMessage"),
                resources.GetString("SearchAddedTitle"));

            dialog.Commands.Add(new UICommand(resources.GetString("Ok"), x => { }, true));
            dialog.Commands.Add(new UICommand(resources.GetString("Cancel"), x => { }, false));

            var result = await dialog.ShowAsync();
            return (bool)result.Id;
        }

        public SearchViewModel ViewModel
        {
            get => (SearchViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SearchViewModel)value;
        }
    }
}