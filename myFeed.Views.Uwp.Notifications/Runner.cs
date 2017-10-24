using System;
using System.IO;
using Autofac;
using myFeed.Services;
using myFeed.Services.Abstractions;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using LiteDB;
using myFeed.Repositories;
using myFeed.Services.Platform;

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
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            builder.Register(x => new LiteDatabase(filePath)).AsSelf().SingleInstance();
            builder.RegisterType<UwpNotificationService>().As<INotificationService>();
            builder.RegisterModule<RepositoriesModule>();
            builder.RegisterModule<ServicesModule>();
            return builder.Build();
        }
    }
}
