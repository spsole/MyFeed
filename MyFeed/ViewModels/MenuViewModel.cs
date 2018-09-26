using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Reflection;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(MenuViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class MenuViewModel : ISupportsActivation
    {
        private readonly Func<Type, string, object, MenuItemViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly IPlatformService _platformService;
        private readonly ISettingManager _settingManager;

        public ReactiveList<MenuItemViewModel> Items { get; }
        public MenuItemViewModel Selection { get; set; }
        public ViewModelActivator Activator { get; }

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

            Activator = new ViewModelActivator();
            this.WhenActivated(async disposables => await Activate(disposables));
        }

        private async Task Activate(CompositeDisposable disposables)
        {
            var settings = await _settingManager.Read();
            settings.Fetched = DateTime.Now;
            
            await _settingManager.Write(settings);
            await _platformService.RegisterBackgroundTask(settings.Period);
            await _platformService.RegisterTheme(settings.Theme);
            await Task.Delay(250);

            _platformService.Icons
                .Select(x => _factory(x.Key, x.Value.Item1, x.Value.Item2))
                .ToList().ForEach(Items.Add);
            Selection = Items.FirstOrDefault();
        }
    }
}
