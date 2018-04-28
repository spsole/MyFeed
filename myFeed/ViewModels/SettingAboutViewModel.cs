using System.Reactive;
using DryIocAttributes;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SettingAboutViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingAboutViewModel
    {
        public ReactiveCommand<Unit, Unit> Feedback { get; }
        public ReactiveCommand<Unit, Unit> Review { get; }
        public string Version { get; }

        public SettingAboutViewModel(IPackagingService packagingService)
        {
            Version = packagingService.Version;
            Feedback = ReactiveCommand.CreateFromTask(packagingService.LeaveFeedback);
            Review = ReactiveCommand.CreateFromTask(packagingService.LeaveReview);
        }
    }
}
