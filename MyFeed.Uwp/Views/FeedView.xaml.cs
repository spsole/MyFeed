using MyFeed.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using System;
using Windows.UI.Xaml.Media;

namespace MyFeed.Uwp.Views
{
    public sealed partial class FeedView : Page, IViewFor<FeedViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FeedViewModel),
                typeof(FeedView), new PropertyMetadata(null));

        public FeedView()
        {
            InitializeComponent();
            DataContextChanged += (a, args) => ViewModel = args.NewValue as FeedViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsEmpty,
                    view => view.ShowCategoriesButton.Visibility,
                    empty => empty ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Selection,
                    view => view.CategoriesListBox.SelectedItem)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Items,
                    view => view.CategoriesListBox.ItemsSource)
                    .DisposeWith(disposables);

                CategoriesListBox.Events().Tapped
                    .Select(args => VisualTreeHelper.GetOpenPopups(Window.Current))
                    .SelectMany(popups => popups)
                    .Where(popup => popup.IsOpen)
                    .Subscribe(popup => popup.IsOpen = false)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Load,
                    view => view.RefreshCategories)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Items,
                    view => view.CategoriesPivot.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                    viewModel => viewModel.Selection,
                    view => view.CategoriesPivot.SelectedItem)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.LoadingScreen.IsActive)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.HasErrors,
                    view => view.HasErrorsScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Load,
                    view => view.HasErrorsScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                   viewModel => viewModel.IsEmpty,
                   view => view.ModifyScreen.IsVisible)
                   .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                   viewModel => viewModel.Modify,
                   view => view.ModifyScreen.Command)
                   .DisposeWith(disposables);
            });
        }

        public FeedViewModel ViewModel
        {
            get => (FeedViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FeedViewModel)value;
        }
    }
}