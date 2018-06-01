using System;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Platform;
using PropertyChanged;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using myFeed.Models;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SettingViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingViewModel 
    {
        private readonly IFilePickerService _filePickerService;
        private readonly IPackagingService _packagingService;
        private readonly IPlatformService _platformService;
        private readonly ISettingManager _settingManager;
        private readonly IOpmlService _opmlService;

        public Interaction<Unit, bool> ImportSuccess { get; }
        public Interaction<Unit, bool> ExportSuccess { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }

        public Interaction<Unit, bool> ResetConfirm { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }

        public ReactiveCommand<Unit, Unit> Feedback { get; }
        public ReactiveCommand<Unit, Unit> Review { get; }
        public string Version { get; }

        public double Font { get; set; }
        public string Theme { get; set; }
        public bool Banners { get; set; }
        public bool Images { get; set; }
        public bool Read { get; set; }
        public int Period { get; set; }
        public int Max { get; set; }

        public SettingViewModel(
            IFilePickerService filePickerService,
            IPackagingService packagingService,
            IPlatformService platformService,
            ISettingManager settingManager,
            IOpmlService opmlService)
        {
            _filePickerService = filePickerService;
            _packagingService = packagingService;
            _platformService = platformService;
            _settingManager = settingManager;
            _opmlService = opmlService;

            Version = _packagingService.Version;
            Feedback = ReactiveCommand.CreateFromTask(packagingService.LeaveFeedback);
            Review = ReactiveCommand.CreateFromTask(packagingService.LeaveReview);

            ImportSuccess = new Interaction<Unit, bool>();
            ExportSuccess = new Interaction<Unit, bool>();
            Import = ReactiveCommand.CreateFromTask(DoImport);
            Export = ReactiveCommand.CreateFromTask(DoExport);

            ResetConfirm = new Interaction<Unit, bool>();
            Reset = ReactiveCommand.CreateFromTask(DoReset);
            _settingManager.Read().ToObservable()
                .Do(x => Banners = x.Banners)
                .Do(x => Images = x.Images)
                .Do(x => Period = x.Period)
                .Do(x => Theme = x.Theme)
                .Do(x => Read = x.Read)
                .Do(x => Font = x.Font)
                .Do(x => Max = x.Max)
                .Subscribe();

            this.WhenAnyValue(x => x.Period).Skip(1)
                .SelectMany(platformService.RegisterBackgroundTask)
                .Subscribe();
            this.WhenAnyValue(x => x.Theme).Skip(1)
                .SelectMany(platformService.RegisterTheme)
                .Subscribe();
            this.WhenAnyValue(
                    x => x.Period, x => x.Theme, 
                    x => x.Banners, x => x.Images,
                    x => x.Read, x => x.Font, x => x.Max)
                .Skip(1).Select(x => new Settings
                {
                    Period = x.Item1, Theme = x.Item2,
                    Banners = x.Item3, Images = x.Item4,
                    Read = x.Item5, Font = x.Item6, Max = x.Item7
                })
                .SelectMany(settingManager.Write)
                .Subscribe();
        }

        private async Task DoImport()
        {
            var stream = await _filePickerService.PickFileForReadAsync();
            var success = await _opmlService.ImportOpml(stream);
            if (success) await ImportSuccess.Handle(Unit.Default);
        }

        private async Task DoExport()
        {
            var stream = await _filePickerService.PickFileForWriteAsync();
            var success = await _opmlService.ExportOpml(stream);
            if (success) await ExportSuccess.Handle(Unit.Default);
        }

        private async Task DoReset()
        {
            var response = await ResetConfirm.Handle(Unit.Default);
            if (response) await _platformService.ResetApp();
        }
    }
}