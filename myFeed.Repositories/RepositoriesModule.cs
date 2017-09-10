using Autofac;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Implementations;

namespace myFeed.Repositories
{
    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationRepository>().As<IConfigurationRepository>();
            builder.RegisterType<ArticlesRepository>().As<IArticlesRepository>();
            builder.RegisterType<SourcesRepository>().As<ISourcesRepository>();
            base.Load(builder);
        }
    }
}