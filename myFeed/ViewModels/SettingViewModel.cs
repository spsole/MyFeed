using System;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SettingViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingViewModel 
    {
        public Interaction<Unit, bool> ImportSuccess { get; }
        public Interaction<Unit, bool> ExportSuccess { get; }
        public Interaction<Unit, bool> ResetConfirm { get; }
        
        public ReactiveCommand<Unit, Unit> LeaveFeedback { get; }
        public ReactiveCommand<Unit, Unit> Review { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        
        public string Version { get; }
        public double Font { get; set; }
        public string Theme { get; set; }
        public bool Banners { get; set; }
        public bool Images { get; set; }
        public int Period { get; set; }
        public bool Read { get; set; }
        public int Max { get; set; }
        
        public SettingViewModel(
            IFilePickerService filePickerService,
            IPackagingService packagingService,
            IPlatformService platformService,
            ISettingManager settingManager,
            IOpmlService opmlService)
        {
            Version = packagingService.Version;
            LeaveFeedback = ReactiveCommand.CreateFromTask(packagingService.LeaveFeedback);
            Review = ReactiveCommand.CreateFromTask(packagingService.LeaveReview);
            ImportSuccess = new Interaction<Unit, bool>();
            Import = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForReadAsync();
                var success = await opmlService.ImportOpmlFeedsAsync(stream);
                if (success) await ImportSuccess.Handle(Unit.Default);
            });
            ExportSuccess = new Interaction<Unit, bool>();
            Export = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForWriteAsync();
                var success = await opmlService.ExportOpmlFeedsAsync(stream);
                if (success) await ExportSuccess.Handle(Unit.Default);
            });
            ResetConfirm = new Interaction<Unit, bool>();
            Reset = ReactiveCommand.CreateFromTask(async () =>
            {
                var response = await ResetConfirm.Handle(Unit.Default);
                if (response) await platformService.ResetApp();
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var settings = await settingManager.Read();
                Track(x => x.Period, (s, x) => s.Period = x, settings.Period, platformService.RegisterBackgroundTask);
                Track(x => x.Theme, (s, x) => s.Theme = x, settings.Theme, platformService.RegisterTheme);
                Track(x => x.Banners, (s, x) => s.Banners = x, settings.Banners);
                Track(x => x.Images, (s, x) => s.Images = x, settings.Images);
                Track(x => x.Read, (s, x) => s.Read = x, settings.Read);
                Track(x => x.Font, (s, x) => s.Font = x, settings.Font);
                Track(x => x.Max, (s, x) => s.Max = x, settings.Max);

                void Track<T>(
                    Expression<Func<SettingViewModel, T>> bind, 
                    Action<Settings, T> assign, T initial,
                    Func<T, Task> callback = null)
                {
                    var memberExpression = (MemberExpression)bind.Body;
                    var property = (PropertyInfo)memberExpression.Member;
                    property.SetValue(this, initial);
                    this.WhenAnyValue(bind)
                        .Do(x => assign.Invoke(settings, x))
                        .Do(async x => await settingManager.Write(settings))
                        .Where(x => callback != null)
                        .Subscribe(async x => await callback(x));
                }
            });
        }
    }
}