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
    [ExportEx(typeof(SettingPersonalViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SettingPersonalViewModel
    {
        public ReactiveCommand<Unit, Unit> Load { get; }

        public string Theme { get; set; }
        public bool Banners { get; set; }
        public double Font { get; set; }
        public bool Images { get; set; }
        public int Period { get; set; }
        public bool Read { get; set; }
        public int Max { get; set; }

        public SettingPersonalViewModel(
            IPlatformService platformService,
            ISettingManager settingManager)
        {
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
                    Expression<Func<SettingPersonalViewModel, T>> bind,
                    Action<Settings, T> assign, T initial,
                    Func<T, Task> callback = null)
                {
                    var memberExpression = (MemberExpression)bind.Body;
                    var property = (PropertyInfo)memberExpression.Member;
                    property.SetValue(this, initial);
                    this.WhenAnyValue(bind).Skip(1)
                        .Do(x => assign.Invoke(settings, x))
                        .Do(async x => await settingManager.Write(settings))
                        .Where(x => callback != null)
                        .Subscribe(async x => await callback(x));
                }
            });
        }
    }
}
