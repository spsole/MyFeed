using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace MyFeed.Uwp.Converters
{
    public sealed class LocaleConverter : IValueConverter
    {
        private static readonly ResourceLoader ResourceLoader = ResourceLoader.GetForViewIndependentUse();
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ResourceLoader.GetString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
