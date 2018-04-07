using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace myFeed.Uwp.Converters
{
    public sealed class LocaleConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, string language)
        {
            return ResourceLoader.GetForViewIndependentUse().GetString((string)parameter);
        }
        
        public object ConvertBack(object value, Type type, object parameter, string language) => null;
    }
}