using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using MyFeed.ViewModels;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System;
using ReactiveUI;
using Windows.UI.Xaml;

namespace MyFeed.Uwp.Views
{
    public sealed partial class SearchItemView : UserControl, IViewFor<SearchItemViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(SearchItemViewModel), 
                typeof(SearchItemView), new PropertyMetadata(null));

        public SearchItemView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as SearchItemViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Title,
                    view => view.TitleTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Description,
                    view => view.DescriptionTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Url,
                    view => view.UrlTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Image,
                    view => view.ImageContainer.Source,
                    str => Uri.IsWellFormedUriString(str, UriKind.Absolute)
                        ? new Uri(str) : null)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsSelected,
                    view => view.IsSelectedCheckBox.IsChecked)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Open,
                    view => view.OpenButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Copy,
                    view => view.CopyButton)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.DataContext)
                    .Where(context => context != null)
                    .Select(ignore => ViewModel.Copied)
                    .Subscribe(interaction => interaction
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

        public SearchItemViewModel ViewModel
        {
            get => (SearchItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SearchItemViewModel)value;
        }
    }
}
