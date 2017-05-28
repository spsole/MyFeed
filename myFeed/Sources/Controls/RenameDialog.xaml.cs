using Windows.UI.Xaml.Controls;

namespace myFeed.Sources.Controls
{
    public sealed partial class RenameDialog
    {
        public RenameDialog() => InitializeComponent();

        /// <summary>
        /// Returns user-provided new title of a category.
        /// </summary>
        public string GetNewCategoryTitle() => CategoryName.Text;
    }
}
