using System;
using DryIocAttributes;
using PropertyChanged;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(MenuItemViewModel))]
    public sealed class MenuItemViewModel
    {
        public string Title { get; }
        public object Icon { get; }
        public Type Type { get; }

        public MenuItemViewModel(Type type, string title, object icon)
        {
            Title = title;
            Icon = icon;
            Type = type;
        }
    }
}
