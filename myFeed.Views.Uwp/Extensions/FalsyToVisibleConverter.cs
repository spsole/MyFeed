using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Extensions
{
    public class FalsyToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TruthyToVisibleConverter.IsDefault(value) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}