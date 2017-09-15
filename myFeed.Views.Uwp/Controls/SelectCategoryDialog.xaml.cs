using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace myFeed.Views.Uwp.Controls
{
    public sealed partial class SelectCategoryDialog : ContentDialog
    {
        public SelectCategoryDialog(IEnumerable<object> items, string title)
        {
            InitializeComponent();
            var resourceLoader = new ResourceLoader();
            PrimaryButtonText = resourceLoader.GetString("Ok");
            SecondaryButtonText = resourceLoader.GetString("Cancel");
            SelectBox.ItemsSource = items;
            Title = title;
        }

        public object Value => SelectBox.SelectedItem;
    }
}
