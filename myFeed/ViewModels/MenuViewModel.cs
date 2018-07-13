using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        private readonly Func<Type, string, object, MenuItemViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly IPlatformService _platformService;
        private readonly ISettingManager _settingManager;

        public ReactiveList<MenuItemViewModel> Items { get; }
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
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            _navigationService.Navigated
                .Where(type => type != Selection?.Type)
                .Select(x => Items.FirstOrDefault(i => i.Type == x))
                .Subscribe(x => Selection = x);

            this.WhenAnyValue(x => x.Selection)
                .Where(selection => selection != null)
                .Select(selection => selection.Type)
                .Where(type => type != _navigationService.CurrentViewModelType)
                .Subscribe(viewModelType => typeof(INavigationService)
                    .GetMethod(nameof(INavigationService.Navigate), new Type[0])?
                    .MakeGenericMethod(viewModelType)
                    .Invoke(_navigationService, null));
        }

        private async Task DoLoad()
        {
            await _navigationService.Navigate<FeedViewModel>();
            var settings = await _settingManager.Read();
            settings.Fetched = DateTime.Now;
            
            await _settingManager.Write(settings);
            await _platformService.RegisterBackgroundTask(settings.Period);
            await _platformService.RegisterTheme(settings.Theme);

            _platformService.Icons
                .Select(x => _factory(x.Key, x.Value.Item1, x.Value.Item2))
                .ToList().ForEach(Items.Add);
            Selection = Items.FirstOrDefault();
        }
    }
}
