using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters
{
    public class TruthyToTransparentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TruthyToVisibleConverter.IsDefault(value) ? 1 : 0.5;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}