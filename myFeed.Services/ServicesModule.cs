using myFeed.Services.Abstractions;
using myFeed.Repositories;
using Autofac;
using myFeed.Services.Implementations;

namespace myFeed.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<RepositoriesModule>();
            builder.RegisterType<SerializationService>().As<ISerializationService>();
            builder.RegisterType<FeedlySearchService>().As<ISearchService>();
            builder.RegisterType<AngleSharpHtmlService>().As<IHtmlService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
            builder.RegisterType<OpmlService>().As<IOpmlService>();
            builder.RegisterType<FeedService>().As<IFeedService>();
            base.Load(builder);
        }
    }
}