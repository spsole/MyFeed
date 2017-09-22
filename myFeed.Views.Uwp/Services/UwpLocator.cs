using System;
using Windows.UI.Xaml;
using Autofac;
using myFeed.Services.Abstractions;
using myFeed.ViewModels;
using myFeed.ViewModels.Implementations;

namespace myFeed.Views.Uwp.Services
{
    /// <summary>
    /// Locator that should be used as ViewModelLocator resource for the whole application.
    /// Represents a bindable source containing factories for all ViewModels that 
    /// application contains. Usage: "{Binding Path=., Source={StaticResource Locator}}"
    /// Remember to declare this as a singleton resource in App.xaml or similar.
    /// </summary>
    public sealed class UwpLocator : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;

        /// <inheritdoc />
        public UwpLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ViewModelsModule>();
            builder.RegisterType<UwpNavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<UwpTranslationsService>().As<ITranslationsService>();
            builder.RegisterType<UwpFilePickerService>().As<IFilePickerService>();
            builder.RegisterType<UwpPlatformService>().As<IPlatformService>();
            builder.RegisterType<UwpDefaultsService>().As<IDefaultsService>();
            builder.RegisterType<UwpDialogService>().As<IDialogService>();
            _lifetimeScope = builder.Build();
        }

        /// <summary>
        /// Returns current locator instance.
        /// </summary>
        public static UwpLocator Current => (UwpLocator)Application.Current.Resources["Locator"];

        /// <summary>
        /// Resolves components from Autofac container in a separate 
        /// ILifetimeScope for each ViewModel.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        public T Resolve<T>() where T : class => _lifetimeScope.Resolve<T>();

        /// <summary>
        /// Disposes internally stored lifetime scope.
        /// </summary>
        public void Dispose() => _lifetimeScope.Dispose();

        #region ViewModel factories for Views

        public SettingsViewModel SettingsViewModel => Resolve<SettingsViewModel>();
        public SourcesViewModel SourcesViewModel => Resolve<SourcesViewModel>();
        public SearchViewModel SearchViewModel => Resolve<SearchViewModel>();
        public FeedViewModel FeedViewModel => Resolve<FeedViewModel>();
        public FaveViewModel FaveViewModel => Resolve<FaveViewModel>();
        public MenuViewModel MenuViewModel => Resolve<MenuViewModel>();

        #endregion
    }
}
