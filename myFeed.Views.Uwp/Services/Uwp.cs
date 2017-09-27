using System;
using Windows.UI.Xaml;
using Autofac;
using myFeed.Services.Abstractions;
using myFeed.ViewModels;

namespace myFeed.Views.Uwp.Services
{
    public class Uwp : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;

        public Uwp() => _lifetimeScope = Load(new ContainerBuilder()).Build();

        private static ContainerBuilder Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ViewModelsModule>();
            builder.RegisterType<UwpNavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<UwpTranslationsService>().As<ITranslationsService>();
            builder.RegisterType<UwpFilePickerService>().As<IFilePickerService>();
            builder.RegisterType<UwpPlatformService>().As<IPlatformService>();
            builder.RegisterType<UwpDefaultsService>().As<IDefaultsService>();
            builder.RegisterType<UwpDialogService>().As<IDialogService>();
            builder.RegisterType<UwpHtmlParserService>().AsSelf();
            builder.RegisterType<UwpLauncherService>().AsSelf();
            builder.RegisterType<UwpLegacyFileService>().AsSelf();
            return builder;
        }

        public object Resolve(Type type) => _lifetimeScope.Resolve(type);

        public T Resolve<T>() => _lifetimeScope.Resolve<T>();

        public void Dispose() => _lifetimeScope?.Dispose();

        public static Uwp Current => (Uwp)Application.Current.Resources["Locator"];
    }
}
