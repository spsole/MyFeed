using System;
using System.Linq.Expressions;
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
        public ReactiveCommand LeaveFeedback { get; }
        public ReactiveCommand LeaveReview { get; }
        public ReactiveCommand ImportOpml { get; }
        public ReactiveCommand ExportOpml { get; }
        public ReactiveCommand Reset { get; }
        public ReactiveCommand Load { get; }
        
        public string Version { get; }
        public string Theme { get; set; }
        public double Font { get; set; }
        public bool Banners { get; set; }
        public bool Images { get; set; }
        public int Period { get; set; }
        public int Max { get; set; }

        public SettingViewModel(
            ITranslationService translationsService,
            IFilePickerService filePickerService,
            IPackagingService packagingService,
            IPlatformService platformService,
            ISettingManager settingManager,
            IDialogService dialogService,
            IOpmlService opmlService)
        {
            Version = packagingService.Version;
            LeaveFeedback = ReactiveCommand.CreateFromTask(packagingService.LeaveFeedback);
            LeaveReview = ReactiveCommand.CreateFromTask(packagingService.LeaveReview);
            ImportOpml = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForReadAsync();
                var success = await opmlService.ImportOpmlFeedsAsync(stream);
                if (success) await dialogService.ShowDialog(
                    translationsService.Resolve(Constants.ImportOpmlSuccess),
                    translationsService.Resolve(Constants.Notification));
            });
            ExportOpml = ReactiveCommand.CreateFromTask(async () =>
            {
                var stream = await filePickerService.PickFileForWriteAsync();
                var success = await opmlService.ExportOpmlFeedsAsync(stream);
                if (success) await dialogService.ShowDialog(
                    translationsService.Resolve(Constants.ExportOpmlSuccess),
                    translationsService.Resolve(Constants.Notification));
            });
            Reset = ReactiveCommand.CreateFromTask(async () =>
            {
                var response = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve(Constants.ResetAppNoRestore),
                    translationsService.Resolve(Constants.Notification));
                if (response) await platformService.ResetApp();
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var settings = await settingManager.Read();
                Track(x => x.Period, (s, x) => s.Period = x, settings.Period, platformService.RegisterBackgroundTask);
                Track(x => x.Theme, (s, x) => s.Theme = x, settings.Theme, platformService.RegisterTheme);
                Track(x => x.Banners, (s, x) => s.Banners = x, settings.Banners);
                Track(x => x.Images, (s, x) => s.Images = x, settings.Images);
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