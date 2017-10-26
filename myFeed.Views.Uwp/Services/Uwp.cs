using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Autofac;
using LiteDB;
using myFeed.Repositories;
using myFeed.Services;
using myFeed.ViewModels;
using myFeed.Services.Platform;

namespace myFeed.Views.Uwp.Services
{
    public class Uwp : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;

        public Uwp() => _lifetimeScope = Load(new ContainerBuilder()).Build();

        private static ContainerBuilder Load(ContainerBuilder builder)
        {
            builder.RegisterModule<RepositoriesModule>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterModule<ViewModelsModule>();

            builder.RegisterType<UwpTranslationsService>().As<ITranslationsService>().SingleInstance();
            builder.RegisterType<UwpNavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<UwpFilePickerService>().As<IFilePickerService>();
            builder.RegisterType<UwpPlatformService>().As<IPlatformService>();
            builder.RegisterType<UwpDialogService>().As<IDialogService>();
            builder.RegisterType<UwpHtmlParserService>().AsSelf();
            builder.RegisterType<UwpLegacyFileService>().AsSelf();

            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            builder.Register(x => new LiteDatabase(filePath)).AsSelf().SingleInstance();
            return builder;
        }

        public static Uwp Current => (Uwp)Application.Current.Resources["Locator"];

        public object Resolve(Type type) => _lifetimeScope.Resolve(type);

        public T Resolve<T>() => _lifetimeScope.Resolve<T>();

        public void Dispose() => _lifetimeScope?.Dispose();
    }
}
