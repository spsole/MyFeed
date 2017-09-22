using Windows.ApplicationModel.Background;
using Autofac;
using myFeed.Repositories;
using myFeed.Services;

namespace myFeed.Views.Uwp.Notifications
{
    public sealed class Runner : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            if (cost == BackgroundWorkCostValue.High) return;
            using (var scope = ConfigureScope())
            {
                var processor = scope.Resolve<Processor>();
                await processor.ProcessFeeds().ConfigureAwait(false);
            }
            defferal.Complete();
        }

        private static ILifetimeScope ConfigureScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<RepositoriesModule>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<Processor>().AsSelf();
            return builder.Build();
        }
    }
}
