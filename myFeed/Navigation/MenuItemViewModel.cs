using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions.Mvvm.Implementation;

namespace myFeed.Navigation
{
    /// <summary>
    /// Represents menu item view model.
    /// </summary>
    public class MenuItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Menu item text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Menu item Symbol icon.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Target page type.
        /// </summary>
        public Type PageType { get; }

        public MenuItemViewModel(Symbol icon, string rlKey, Type pageType)
        {
            var resourceLoader = new ResourceLoader();
            Icon = char.ConvertFromUtf32((int)icon);
            Text = resourceLoader.GetString(rlKey);
            PageType = pageType;
        }
    }
}
