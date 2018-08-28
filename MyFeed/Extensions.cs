using DryIoc.MefAttributedModel;
using DryIoc;

namespace MyFeed
{
    public static class Extensions 
    {
        public static void RegisterShared(this IContainer container)
        {
            container.WithMefAttributedModel();
            container.RegisterExports(new [] { typeof(Extensions).GetAssembly() });
        }
    }
}
