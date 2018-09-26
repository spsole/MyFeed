using MyFeed.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System;

namespace MyFeed.Uwp.Views
{
    public sealed partial class ChannelItemView : UserControl, IViewFor<ChannelItemViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ChannelItemViewModel),
                   typeof(ChannelItemView), new PropertyMetadata(null));

        public ChannelItemView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as ChannelItemViewModel;
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Notify,
                    view => view.NotifyToggleSwitch.IsOn)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Name,
                    view => view.NameTextBlock.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Url,
                    view => view.UrlTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Copy,
                    view => view.CopyButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Open,
                    view => view.OpenButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Delete,
                    view => view.DeleteButton)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel)
                    .Where(viewModel => viewModel != null)
                    .Subscribe(viewModel => viewModel.Copied
                        .RegisterHandler(async x => x.SetOutput(await HandleCopied()))
                        .DisposeWith(disposables))
                    .DisposeWith(disposables);
            });
        }

        private async Task<bool> HandleCopied()
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var messageDialog = new MessageDialog(
                resourceLoader.GetString("SearchCopiedMessage"),
                resourceLoader.GetString("SearchCopiedTitle"));

            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Ok"), x => { }, true));
            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel"), x => { }, false));

            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public ChannelItemViewModel ViewModel
        {
            get => (ChannelItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ChannelItemViewModel)value;
        }
    }
}
