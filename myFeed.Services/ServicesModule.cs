using myFeed.Services.Abstractions;
using Autofac;
using CodeHollow.FeedReader;
using myFeed.Repositories;
using myFeed.Services.Implementations;

namespace myFeed.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<RepositoriesModule>();
            builder.RegisterType<SerializationService>().As<ISerializationService>();
            builder.RegisterType<FeedReaderFetchService>().As<IFeedFetchService>();
            builder.RegisterType<FeedStoreService>().As<IFeedStoreService>();
            builder.RegisterType<FeedlySearchService>().As<ISearchService>();
            builder.RegisterType<AngleSharpHtmlService>().As<IHtmlService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
            builder.RegisterType<OpmlService>().As<IOpmlService>();
            base.Load(builder);
        }
    }
}