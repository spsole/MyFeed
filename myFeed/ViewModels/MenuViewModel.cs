﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            INavigationService navigationService,
            IPlatformService platformService,
            IFactoryService factoryService,
            ISettingManager settingManager)
        {
            Items = new ReactiveList<MenuItemViewModel>();
            Navigate = ReactiveCommand.Create(() =>
            {
                if (Selection == null) return;
                typeof(INavigationService).GetMethod("Navigate", new Type[] {})
                    .MakeGenericMethod(Selection.Type)
                    .Invoke(navigationService, null);
            });
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var settings = await settingManager.Read();
                settings.Fetched = DateTime.Now;
                await settingManager.Write(settings);
                await platformService.RegisterTheme(settings.Theme);
                await platformService.RegisterBackgroundTask(settings.Period);
                await navigationService.Navigate<FeedViewModel>();

                var factory = factoryService.Create<Func<Type, string, object, MenuItemViewModel>>();
                foreach (var item in new Dictionary<Type, string>
                {
                    {typeof(FeedViewModel), Constants.FeedViewMenuItem},
                    {typeof(FaveViewModel), Constants.FaveViewMenuItem},
                    {typeof(ChannelViewModel), Constants.ChannelsViewMenuItem},
                    {typeof(SearchViewModel), Constants.SearchViewMenuItem},
                    {typeof(SettingViewModel), Constants.SettingsViewMenuItem}
                }) 
                    Items.Add(factory(item.Key, item.Value, navigationService.Icons[item.Key]));

                Selection = Items.FirstOrDefault();
                navigationService.Navigated += (sender, args) =>
                {
                    if (args == Selection?.Type) return;
                    Selection = Items.FirstOrDefault(x => x.Type == args);
                };
            });
        }
    }
}
