using Autofac;
using myFeed.Services.Abstractions;
using myFeed.Services.Implementations;

namespace myFeed.Services
{
    public sealed class ServicesModule : Module 
    {
        protected override void Load(ContainerBuilder builder) 
        {
            builder.RegisterType<CachingSettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<AutofacFactoryService>().As<IFactoryService>().SingleInstance();
            builder.RegisterType<XmlSerializationService>().As<ISerializationService>();
            builder.RegisterType<ParallelFeedStoreService>().As<IFeedStoreService>();
            builder.RegisterType<FeedReaderFetchService>().As<IFeedFetchService>();
            builder.RegisterType<FeedlySearchService>().As<ISearchService>();
            builder.RegisterType<FavoritesService>().As<IFavoritesService>();
            builder.RegisterType<RegexImageService>().As<IImageService>();
            builder.RegisterType<OpmlService>().As<IOpmlService>();
        }
    }
}