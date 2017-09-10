using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters
{
    public class FalsyToVisibleConverter : IValueConverter
    {
        public object ConvertBack(object value, Type target, object o, string lang) => null;

        public object Convert(object value, Type target, object o, string lang) =>
            value.IsDefault() ? Visibility.Visible : Visibility.Collapsed;
    }
}