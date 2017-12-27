using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Uwp.Converters
{
    public class UpperStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((string) value).ToUpperInvariant();
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}