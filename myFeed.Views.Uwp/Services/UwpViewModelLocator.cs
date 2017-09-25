using System;
using Windows.UI.Xaml;
using Autofac;
using myFeed.Services.Abstractions;
using myFeed.ViewModels;

namespace myFeed.Views.Uwp.Services
{
    /// <summary>
    /// Locator that should be used as ViewModelLocator resource for the whole application.
    /// Represents a bindable source containing factories for all ViewModels that 
    /// application contains. Usage: "{Binding Path=., Source={StaticResource Locator}}"
    /// Remember to declare this as a singleton resource in App.xaml or similar.
    /// </summary>
    public class UwpViewModelLocator : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope; 

        /// <inheritdoc />
        public UwpViewModelLocator() => _lifetimeScope = Load(new ContainerBuilder()).Build();

        /// <summary>
        /// Returns current locator instance.
        /// </summary>
        public static UwpViewModelLocator Current => (UwpViewModelLocator)Application.Current.Resources["Locator"];

        /// <summary>
        /// Resolves generic from scope.
        /// </summary>
        public T Resolve<T>() => _lifetimeScope.Resolve<T>();

        /// <summary>
        /// Resolves type from scope.
        /// </summary>
        public object Resolve(Type type) => _lifetimeScope.Resolve(type);

        /// <summary>
        /// Disposes internally stored lifetime scope.
        /// </summary>
        public void Dispose() => _lifetimeScope?.Dispose();

        /// <summary>
        /// Loads all registrations into container builder.
        /// </summary>
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
            return builder;
        }
    }
}
