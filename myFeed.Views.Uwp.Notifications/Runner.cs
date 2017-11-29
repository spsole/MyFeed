using System;
using System.IO;
using Autofac;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using LiteDB;
using myFeed.Services;
using myFeed.Services.Platform;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Notifications
{
    public sealed class Runner : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            try { using (var scope = Load(new ContainerBuilder()))
                      await scope.Resolve<IBackgroundService>()
                          .CheckForUpdates(DateTime.Now); }
            catch { /* ignored */ }
            finally { defferal.Complete(); }
        }

        private static ILifetimeScope Load(ContainerBuilder builder)
        {
            var connection = "MyFeed.db";
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, connection);
            builder.Register(x => new LiteDatabase(filePath)).AsSelf().SingleInstance();
            builder.RegisterType<UwpNotificationService>().As<INotificationService>();
            builder.RegisterModule<ServicesModule>();
            return builder.Build();
        }
    }
}
