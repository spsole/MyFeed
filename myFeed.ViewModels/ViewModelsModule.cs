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
            builder.RegisterType<ArticleViewModel>().AsSelf();
            builder.RegisterType<ChannelCategoryViewModel>().AsSelf();
            builder.RegisterType<ChannelsViewModel>().AsSelf();
            builder.RegisterType<ChannelViewModel>().AsSelf();
            builder.RegisterType<FaveViewModel>().AsSelf();
            builder.RegisterType<FeedCategoryViewModel>().AsSelf();
            builder.RegisterType<FeedViewModel>().AsSelf();
            builder.RegisterType<MenuViewModel>().AsSelf();
            builder.RegisterType<SearchItemViewModel>().AsSelf();
            builder.RegisterType<SearchViewModel>().AsSelf();
            builder.RegisterType<SettingsViewModel>().AsSelf();
        }
    }
}