using DryIoc;
using DryIoc.MefAttributedModel;

namespace myFeed.Services
{
    public static class Extensions 
    {
        public static void RegisterServices(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            var assembly = typeof(Extensions).GetAssembly();
            registrator.RegisterExports(new [] {assembly});
        }
    }
}