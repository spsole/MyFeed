using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Extensions
{
    public class NegateBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool) value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(bool) value;
        }
    }
}