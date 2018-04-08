using DryIoc.MefAttributedModel;
using DryIoc;
using Reactive.EventAggregator;

namespace myFeed
{
    public static class Extensions 
    {
        public static void RegisterShared(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            registrator.RegisterExports(new [] { typeof(Extensions).GetAssembly() });
            registrator.Register<IEventAggregator, EventAggregator>(Reuse.Singleton);
        }
    }
}