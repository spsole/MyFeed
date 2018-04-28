using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SettingSpecialViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingSpecialViewModel
    {
        public Interaction<Unit, bool> ImportSuccess { get; }
        public Interaction<Unit, bool> ExportSuccess { get; }
        public Interaction<Unit, bool> ResetConfirm { get; }

        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }

        public SettingSpecialViewModel(
            IFilePickerService filePickerService,
            IPlatformService platformService,
            IOpmlService opmlService)
        {
            ImportSuccess = new Interaction<Unit, bool>();
            Import = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForReadAsync();
                var success = await opmlService.ImportOpml(stream);
                if (success) await ImportSuccess.Handle(Unit.Default);
            });

            ExportSuccess = new Interaction<Unit, bool>();
            Export = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForWriteAsync();
                var success = await opmlService.ExportOpml(stream);
                if (success) await ExportSuccess.Handle(Unit.Default);
            });

            ResetConfirm = new Interaction<Unit, bool>();
            Reset = ReactiveCommand.CreateFromTask(async () =>
            {
                var response = await ResetConfirm.Handle(Unit.Default);
                if (response) await platformService.ResetApp();
            });
        }
    }
}
