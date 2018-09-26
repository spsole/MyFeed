using MyFeed.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Disposables;

namespace MyFeed.Uwp.Views
{
    public sealed partial class ArticleView : Page, IViewFor<FeedItemViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FeedItemViewModel),
                typeof(ArticleView), new PropertyMetadata(null));

        public ArticleView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as FeedItemViewModel;
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, 
                    viewModel => viewModel.Launch, 
                    view => view.ArticleLaunchButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, 
                    viewModel => viewModel.Copy, 
                    view => view.ArticleCopyButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, 
                    viewModel => viewModel.Share, 
                    view => view.ArticleShareButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, 
                    viewModel => viewModel.MarkFave, 
                    view => view.ArticleStarButton)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fave,
                    view => view.ArticleStarButton.Visibility,
                    fave => fave ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.MarkFave, 
                    view => view.ArticleUnstarButton)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Fave,
                    view => view.ArticleUnstarButton.Visibility,
                    fave => fave ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Feed,
                    view => view.Feed.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Title,
                    view => view.Title.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Published,
                    view => view.DateRun.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Content,
                    view => view.HtmlContent.Html)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Font,
                    view => view.HtmlContent.HtmlFontSize)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Images,
                    view => view.HtmlContent.Images)
                    .DisposeWith(disposables);
            });
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
