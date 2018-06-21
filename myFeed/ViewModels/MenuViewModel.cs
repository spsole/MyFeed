using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        private readonly Func<Type, string, object, MenuItemViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly IPlatformService _platformService;
        private readonly ISettingManager _settingManager;

        public ReactiveList<MenuItemViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> Navigate { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public MenuItemViewModel Selection { get; set; }

        public MenuViewModel(
            Func<Type, string, object, MenuItemViewModel> factory,
            INavigationService navigationService,
            IPlatformService platformService,
            ISettingManager settingManager)
        {
            _navigationService = navigationService;
            _platformService = platformService;
            _settingManager = settingManager;
            _factory = factory;

            Items = new ReactiveList<MenuItemViewModel>();
            Navigate = ReactiveCommand.Create(DoNavigate);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            _navigationService.Navigated
                .Where(x => x != Selection?.Type)
                .Select(x => Items.FirstOrDefault(i => i.Type == x))
                .Subscribe(x => Selection = x);
        }

        private async Task DoLoad()
        {
            await _navigationService.Navigate<FeedViewModel>();
            var settings = await _settingManager.Read();
            settings.Fetched = DateTime.Now;

            await _platformService.RegisterBackgroundTask(settings.Period);
            await _platformService.RegisterTheme(settings.Theme);
            await _settingManager.Write(settings);

            _navigationService.Icons.ToList().ForEach(item => 
                Items.Add(_factory(item.Key, item.Value.Item1, item.Value.Item2)
            ));
            Selection = Items.FirstOrDefault();
        }

        private void DoNavigate()
        {
            if (Selection == null) return;
            typeof(INavigationService)
                .GetMethod("Navigate", new Type[] { })
                .MakeGenericMethod(Selection.Type)
                .Invoke(_navigationService, null);
        }
    }
}
