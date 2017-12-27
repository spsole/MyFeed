using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using DryIoc;
using DryIoc.MefAttributedModel;
using LiteDB;

namespace myFeed.Uwp.Services
{
    public class Uwp
    {
        private readonly IResolver _resolver;

        public Uwp()
        {
            var container = new Container();
            container.RegisterShared();
            container.RegisterExports(new[] { typeof(Uwp).GetAssembly() });
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            _resolver = container.WithNoMoreRegistrationAllowed();
        }

        public static Uwp Current => (Uwp)Application.Current.Resources["Locator"];

        public object Resolve(Type type) => _resolver.Resolve(type);

        public T Resolve<T>() => _resolver.Resolve<T>();
    }
}
