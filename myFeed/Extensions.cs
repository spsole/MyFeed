using DryIoc.MefAttributedModel;
using DryIoc;

namespace myFeed
{
    public static class Extensions 
    {
        public static void RegisterShared(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            var assembly = typeof(Extensions).GetAssembly();
            registrator.RegisterExports(new [] {assembly});
        }
    }
}