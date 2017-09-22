using Windows.ApplicationModel.Background;
using Autofac;
using myFeed.Services;
using myFeed.Services.Abstractions;
using myFeed.Views.Uwp.Notifications.Services;

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
                var processor = scope.Resolve<UwpFeedProcessor>();
                await processor.ProcessFeeds().ConfigureAwait(false);
            }
            defferal.Complete();
        }

        private static ILifetimeScope ConfigureScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<UwpDefaultsService>().As<IDefaultsService>();
            builder.RegisterType<UwpFeedProcessor>().AsSelf();
            return builder.Build();
        }
    }
}
