using System;
using DryIocAttributes;
using myFeed.Platform;
using PropertyChanged;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(MenuItemViewModel))]
    public sealed class MenuItemViewModel
    {
        public string Title { get; }
        public object Icon { get; }
        public Type Type { get; }

        public MenuItemViewModel(
            ITranslationService translationService,
            Type type, string key, object icon)
        {
            Title = translationService.Resolve(key);
            (Icon, Type) = (icon, type);
        }
    }
}
