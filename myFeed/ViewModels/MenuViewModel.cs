using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(MenuViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class MenuViewModel
    {
        public ReactiveList<MenuItemViewModel> Items { get; }
        public MenuItemViewModel Selection { get; set; }
        public ReactiveCommand Navigate { get; }
        public ReactiveCommand Load { get; }

        public MenuViewModel(
            Func<Type, string, object, MenuItemViewModel> factory,
            INavigationService navigationService,
            IPlatformService platformService,
            ISettingManager settingManager)
        {
            Items = new ReactiveList<MenuItemViewModel>();
            Navigate = ReactiveCommand.Create(() =>
            {
                if (Selection == null) return;
                typeof(INavigationService)
                    .GetMethod("Navigate", new Type[] {})
                    .MakeGenericMethod(Selection.Type)
                    .Invoke(navigationService, null);
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                await navigationService.Navigate<FeedViewModel>();
                var settings = await settingManager.Read();
                settings.Fetched = DateTime.Now;

                await platformService.RegisterBackgroundTask(settings.Period);
                await platformService.RegisterTheme(settings.Theme);
                await settingManager.Write(settings);

                navigationService.Icons.ToList().ForEach(item => Items.Add(
                    factory(item.Key, item.Value.Item1, item.Value.Item2)));
                
                Selection = Items.FirstOrDefault();
                navigationService.Navigated
                    .Where(x => x != Selection?.Type)
                    .Select(x => Items.FirstOrDefault(i => i.Type == x))
                    .Subscribe(x => Selection = x);                
            });
        }
    }
}
