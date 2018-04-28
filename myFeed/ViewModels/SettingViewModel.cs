using DryIocAttributes;
using PropertyChanged;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SettingViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingViewModel 
    {
        public SettingPersonalViewModel Personal { get; }
        public SettingSpecialViewModel Special { get; }
        public SettingAboutViewModel About { get; }

        public SettingViewModel(
            SettingPersonalViewModel settingPersonalViewModel,
            SettingSpecialViewModel settingSpecialViewModel,
            SettingAboutViewModel settingAboutViewModel)
        {
            Personal = settingPersonalViewModel;
            Special = settingSpecialViewModel;
            About = settingAboutViewModel;
        }
    }
}