using Autofac;
using myFeed.Services.Abstractions;
using myFeed.Services.Implementations;

namespace myFeed.Services
{
    public sealed class ServicesModule : Module 
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LiteDbCategoryStoreService>().As<ICategoryStoreService>();
            builder.RegisterType<LiteDbFavoriteStoreService>().As<IFavoriteStoreService>();
            builder.RegisterType<LiteDbSettingStoreService>().As<ISettingStoreService>();
            
            builder.RegisterType<CacheableSettingService>().As<ISettingService>().SingleInstance();
            builder.RegisterType<AutofacFactoryService>().As<IFactoryService>().SingleInstance();
            builder.RegisterType<XmlSerializationService>().As<ISerializationService>();
            builder.RegisterType<ParallelFeedStoreService>().As<IFeedStoreService>();
            builder.RegisterType<FeedReaderFetchService>().As<IFeedFetchService>();
            builder.RegisterType<BackgroundService>().As<IBackgroundService>();
            builder.RegisterType<FeedlySearchService>().As<ISearchService>();
            builder.RegisterType<FavoriteService>().As<IFavoriteService>();
            builder.RegisterType<DefaultsService>().As<IDefaultsService>();
            builder.RegisterType<RegexImageService>().As<IImageService>();
            builder.RegisterType<OpmlService>().As<IOpmlService>();
        }
    }
}