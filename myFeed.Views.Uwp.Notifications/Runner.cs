using System;
using Autofac;
using myFeed.Services;
using myFeed.Services.Abstractions;
using Windows.ApplicationModel.Background;

namespace myFeed.Views.Uwp.Notifications
{
    public sealed class Runner : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            using (var scope = Load(new ContainerBuilder()))
            {
                var processor = scope.Resolve<IBackgroundService>();
                await processor.CheckForUpdates(DateTime.Now);
            }
            defferal.Complete();
        }

        private static ILifetimeScope Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<UwpNotificationService>().AsSelf();
            return builder.Build();
        }
    }
}
