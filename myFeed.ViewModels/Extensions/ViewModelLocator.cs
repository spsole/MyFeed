using System;
using Autofac;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Implementations;

namespace myFeed.ViewModels.Extensions
{
    /// <summary>
    /// Locator that should be used as ViewModel Locator resource for the whole application.
    /// Represents a bindable source containing factories for all ViewModels that 
    /// application contains. Usage: "{Binding Path=., Source={StaticResource Locator}}"
    /// Remember to declare this as a singleton resource in App.xaml or similar.
    /// </summary>
    public abstract class ViewModelLocator<TA, TB, TC, TD> : IDisposable
        where TA : ITranslationsService
        where TB : IPlatformService
        where TC : IDialogService
        where TD : IFilePickerService
    {
        private readonly IContainer _lifetimeScope;

        /// <summary>
        /// Initializes new ViewModels abstract locator.
        /// </summary>
        protected ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ViewModelsModule>();
            builder.RegisterType<TA>().As<ITranslationsService>();
            builder.RegisterType<TB>().As<IPlatformService>();
            builder.RegisterType<TC>().As<IDialogService>();
            builder.RegisterType<TD>().As<IFilePickerService>();
            _lifetimeScope = builder.Build();
        }

        /// <summary>
        /// Resolves components from Autofac container in a separate 
        /// ILifetimeScope for each ViewModel.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        protected T Resolve<T>() where T : class => _lifetimeScope.Resolve<T>();

        /// <summary>
        /// Disposes internally stored lifetime scope.
        /// </summary>
        public void Dispose() => _lifetimeScope.Dispose();

        #region ViewModel factories for Views!

        public SettingsViewModel SettingsViewModel => Resolve<SettingsViewModel>();
        public SourcesViewModel SourcesViewModel => Resolve<SourcesViewModel>();
        public SearchViewModel SearchViewModel => Resolve<SearchViewModel>();
        public FeedViewModel FeedViewModel => Resolve<FeedViewModel>();
        public FaveViewModel FaveViewModel => Resolve<FaveViewModel>();

        #endregion
    }
}