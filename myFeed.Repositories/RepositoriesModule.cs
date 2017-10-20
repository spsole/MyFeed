using Autofac;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Implementations;
using LiteDB;

namespace myFeed.Repositories
{
    public sealed class RepositoriesModule : Module 
    {
        protected override void Load(ContainerBuilder builder) 
        {
            builder.RegisterType<CategoriesRepository>().As<ICategoriesRepository>().SingleInstance();
            builder.RegisterType<FavoritesRepository>().As<IFavoritesRepository>().SingleInstance();
            builder.RegisterType<SettingsRepository>().As<ISettingsRepository>().SingleInstance();
        }
    }
}