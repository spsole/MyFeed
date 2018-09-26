using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Resources;
using MyFeed.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using Windows.UI.Xaml.Media.Animation;

namespace MyFeed.Uwp.Views
{
    public sealed partial class FeedItemView : UserControl, IViewFor<FeedItemViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FeedItemViewModel),
                typeof(FeedItemView), new PropertyMetadata(0));

        public FeedItemView()
        {
            InitializeComponent();
            DataContextChanged += (a, args) => ViewModel = args.NewValue as FeedItemViewModel;
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.Read, x => x.ViewModel.Fave)
                    .Select(x => x.Item1 && !x.Item2 ? 0.5 : 1.0)
                    .BindTo(this, view => view.LayoutGrid.Opacity)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Open,
                    view => view.OpenItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Copy,
                    view => view.CopyItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Launch,
                    view => view.LaunchItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Share,
                    view => view.ShareItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.MarkRead,
                    view => view.MarkReadItem)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                    viewModel => viewModel.MarkRead,
                    view => view.MarkUnreadItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.MarkFave,
                    view => view.StarItem)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                    viewModel => viewModel.MarkFave,
                    view => view.UnstarItem)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Read, x => x.ViewModel.Fave)
                    .Select(x => x.Item2 ? Visibility.Collapsed : 
                                 x.Item1 ? Visibility.Collapsed : Visibility.Visible)
                    .BindTo(this, view => view.MarkReadItem.Visibility)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Read, x => x.ViewModel.Fave)
                    .Select(x => x.Item2 ? Visibility.Collapsed : 
                                 x.Item1 ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, view => view.MarkUnreadItem.Visibility)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fave,
                    view => view.StarItem.Visibility,
                    fave => fave ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fave,
                    view => view.UnstarItem.Visibility,
                    Fave => Fave ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(disposables);

                var defaultUri = new Uri("ms-appx:///Assets/StoreLogo.png");
                BitmapImage.UriSource = defaultUri;
                var imageUriObservable = this
                    .WhenAnyValue(x => x.ViewModel)
                    .Where(model => model != null)
                    .Select(model => model.Image);

                imageUriObservable
                    .Where(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .Select(image => new Uri(image))
                    .BindTo(this, view => view.BitmapImage.UriSource)
                    .DisposeWith(disposables);
                imageUriObservable
                    .Where(x => !Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .Select(image => defaultUri)
                    .BindTo(this, view => view.BitmapImage.UriSource)
                    .DisposeWith(disposables);
                Observable
                    .FromEventPattern(BitmapImage, nameof(BitmapImage.ImageOpened))
                    .Subscribe(ignore => ((Storyboard)Resources["PictureStoryboard"]).Begin())
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fave,
                    view => view.FaveTextBlock.Visibility,
                    fave => fave ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Title,
                    view => view.TitleTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Feed,
                    view => view.FeedTextBlock.Text,
                    feed => feed.ToUpperInvariant())
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Published,
                    view => view.PublishedTextBlock.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel)
                    .Where(viewModel => viewModel != null)
                    .Subscribe(viewModel =>
                    {
                        viewModel.Copied
                            .RegisterHandler(async x => x.SetOutput(await HandleCopied()))
                            .DisposeWith(disposables);

                        this.Events().Tapped
                            .Select(routedEventArgs => Unit.Default)
                            .InvokeCommand(ViewModel.Open)
                            .DisposeWith(disposables);

                        Observable.FromEventPattern(LayoutGrid, nameof(Holding))
                            .Merge(Observable.FromEventPattern(LayoutGrid, nameof(RightTapped)))
                            .Select(args => args.Sender as FrameworkElement)
                            .Subscribe(FlyoutBase.ShowAttachedFlyout)
                            .DisposeWith(disposables);
                    })
                    .DisposeWith(disposables);
            });
        }

        private async Task<bool> HandleCopied()
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var messageDialog = new MessageDialog(
                resourceLoader.GetString("CopyLinkSuccess"),
                resourceLoader.GetString("SettingsNotification"));

            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Ok"), x => { }, true));
            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel"), x => { }, false));

            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public FeedItemViewModel ViewModel
        {
            get => (FeedItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FeedItemViewModel)value;
        }
    }
}
