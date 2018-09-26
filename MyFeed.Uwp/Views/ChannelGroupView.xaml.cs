using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MyFeed.ViewModels;
using ReactiveUI;

namespace MyFeed.Uwp.Views
{
    public sealed partial class ChannelGroupView : UserControl, IViewFor<ChannelGroupViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ChannelGroupViewModel),
                   typeof(ChannelGroupView), new PropertyMetadata(null));

        public ChannelGroupView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as ChannelGroupViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.RealTitle,
                    view => view.RealTitleTextBlock.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Channels.Count,
                    view => view.ChannelsCountTextBlock.Text)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Title,
                    view => view.TitleTextBox.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Remove,
                    view => view.RemoveButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.ChannelUri,
                    view => view.ChannelUriTextBox.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.CreateChannel,
                    view => view.CreateChannelButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Channels,
                    view => view.ChannelsListView.ItemsSource)
                    .DisposeWith(disposables);
            });
        }

        public ChannelGroupViewModel ViewModel
        {
            get => (ChannelGroupViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ChannelGroupViewModel)value;
        }
    }
}
