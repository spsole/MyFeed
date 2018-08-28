using System;
using Windows.UI.Xaml.Data;

namespace MyFeed.Uwp.Converters
{
    public sealed class UpperStringConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, string language)
        {
            return ((string) value)?.ToUpperInvariant();
        }

        public object ConvertBack(object value, Type type, object parameter, string language)
        {
            throw new NotImplementedException();    
        }
    }
}