using System;
using System.IO;
using DryIoc;
using LiteDB;
using Windows.Storage;
using Windows.ApplicationModel.Background;
using myFeed.Services.Abstractions;
using DryIoc.MefAttributedModel;
using myFeed.Services;

namespace myFeed.Views.Uwp.Notifications
{
    public sealed class Runner : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            try { using (var scope = Load(new Container()))
                      await scope.Resolve<IBackgroundService>()
                          .CheckForUpdates(DateTime.Now); }
            catch { /* ignored */ }
            finally { defferal.Complete(); }
        }

        private static IContainer Load(IContainer container)
        {
            var connection = "MyFeed.db";
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, connection);

            container.RegisterServices();
            container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            container.RegisterExports(new[] {typeof(Runner).GetType().GetAssembly()});
            return container;
        }
    }
}
