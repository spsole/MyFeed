using MyFeed.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyFeed.Uwp.Views
{
    public sealed partial class FaveView : Page, IViewFor<FaveViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FaveViewModel),
                typeof(FaveView), new PropertyMetadata(null));

        public FaveView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as FaveViewModel;
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel,
                    viewModel => viewModel.OrderByDate,
                    view => view.OrderByDateItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.OrderByFeed,
                    view => view.OrderByFeedItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.OrderByMonth,
                    view => view.OrderByMonthItem)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Items,
                    view => view.GrouppedCollection.Source)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.LoadingScreen.IsActive)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsEmpty,
                    view => view.ReadFeedsScreen.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.ReadFeeds,
                    view => view.ReadFeedsScreen.Command)
                    .DisposeWith(disposables);
            });
        }

        public FaveViewModel ViewModel
        {
            get => (FaveViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FaveViewModel)value;
        }
    }
}