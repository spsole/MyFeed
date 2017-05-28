using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Extensions.Converters
{
    public class UpperStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => 
            ((string) value).ToUpperInvariant();

        public object ConvertBack(object value, Type targetType, object parameter, string language) => 
            throw new NotImplementedException();
    }
}
