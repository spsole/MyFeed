using DryIoc.MefAttributedModel;
using DryIoc;

namespace myFeed
{
    public static class Extensions 
    {
        public static void RegisterShared(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            registrator.RegisterExports(new [] { typeof(Extensions).GetAssembly() });
        }
    }
}