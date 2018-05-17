using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace myFeed.Uwp.Converters
{
    [Bindable]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public sealed class Locale : MarkupExtension
    {
        private static readonly ResourceLoader ResourceLoader = ResourceLoader.GetForViewIndependentUse();

        protected override object ProvideValue() => ResourceLoader.GetString(Key);

        public string Key { get; set; }

        public Locale() { }
    }
}
