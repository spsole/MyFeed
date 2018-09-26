using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MyFeed.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace MyFeed.Uwp.Views
{
    public sealed partial class FeedGroupView : UserControl, IViewFor<FeedGroupViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FeedGroupViewModel),
                typeof(FeedGroupView), new PropertyMetadata(null));

        public FeedGroupView()
        {
            InitializeComponent();
            DataContextChanged += (a, args) => ViewModel = args.NewValue as FeedGroupViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Items,
                    view => view.ItemsListView.ItemsSource)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fetch,
                    view => view.ItemsListView.RefreshCommand)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsEmpty,
                    view => view.ModifyScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Modify,
                    view => view.ModifyScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.HasErrors,
                    view => view.FetchScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fetch,
                    view => view.FetchScreen.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.LoadingScreen.IsActive)
                    .DisposeWith(disposables);
            });
        }

        public FeedGroupViewModel ViewModel
        {
            get => (FeedGroupViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FeedGroupViewModel)value;
        }
    }
}
