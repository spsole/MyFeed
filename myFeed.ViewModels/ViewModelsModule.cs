using Autofac;
using myFeed.Services;
using myFeed.ViewModels.Implementations;

namespace myFeed.ViewModels
{
    public sealed class ViewModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<SettingsViewModel>().AsSelf();
            builder.RegisterType<SourcesViewModel>().AsSelf();
            builder.RegisterType<SearchViewModel>().AsSelf();
            builder.RegisterType<FeedViewModel>().AsSelf();
            builder.RegisterType<FaveViewModel>().AsSelf();
            base.Load(builder);
        }
    }
}