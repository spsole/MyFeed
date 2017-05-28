using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Search.Controls
{
    public sealed partial class SearchAddDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SearchAddDialogViewModel),
                typeof(SearchAddDialog),
                new PropertyMetadata(null)
            );

        public SearchAddDialogViewModel ViewModel
        {
            get => (SearchAddDialogViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public SearchAddDialog(SearchAddDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void OnPrimaryButtonClick(ContentDialog sender, 
            ContentDialogButtonClickEventArgs args) => ViewModel.AddModel();
    }
}
