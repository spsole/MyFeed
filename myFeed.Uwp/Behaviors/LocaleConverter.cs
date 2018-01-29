using System;
using Windows.UI.Xaml.Data;
using DryIoc;
using myFeed.Platform;

namespace myFeed.Uwp.Behaviors
{
    public sealed class LocaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return App.Container.Resolve<ITranslationService>().Resolve((string)parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}