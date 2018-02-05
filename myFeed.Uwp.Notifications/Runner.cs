using System;
using System.Diagnostics;
using System.IO;
using DryIoc;
using LiteDB;
using Windows.Storage;
using Windows.ApplicationModel.Background;
using DryIoc.MefAttributedModel;
using myFeed.Interfaces;

namespace myFeed.Uwp.Notifications
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
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            container.RegisterShared();
            container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            container.RegisterExports(new[] {typeof(Runner).GetAssembly()});
            return container;
        }
    }
}
