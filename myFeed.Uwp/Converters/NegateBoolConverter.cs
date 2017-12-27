using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Uwp.Converters
{
    public class NegateBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => !(bool) value;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => !(bool) value;
    }
}