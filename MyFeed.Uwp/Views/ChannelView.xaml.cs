using MyFeed.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Disposables;
using ReactiveUI;

namespace MyFeed.Uwp.Views
{
    public sealed partial class ChannelView : Page, IViewFor<ChannelViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(ChannelViewModel),
                typeof(ChannelView), new PropertyMetadata(null));

        public ChannelView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as ChannelViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Categories.Count,
                    view => view.CategoriesCountTitleBlock.Count)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Read,
                    view => view.ReadButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                    viewModel => viewModel.Search,
                    view => view.SearchButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.CategoryName,
                    view => view.CategoryNameTextBox.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.CreateCategory,
                    view => view.CreateCategoryButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Categories,
                    view => view.CategoriesListView.ItemsSource)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsEmpty,
                    view => view.CreateCategoryScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.CreateCategory,
                    view => view.CreateCategoryScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.LoadingScreen.IsActive)
                    .DisposeWith(disposables);
            });
        }

        public ChannelViewModel ViewModel
        {
            get => (ChannelViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ChannelViewModel)value;
        }
    }
}