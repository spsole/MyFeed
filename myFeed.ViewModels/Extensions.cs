using DryIoc;
using DryIoc.MefAttributedModel;

namespace myFeed.ViewModels
{
    public static class Extensions
    {
        public static void RegisterViewModels(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            var assembly = typeof(Extensions).GetAssembly();
            registrator.RegisterExports(new [] {assembly});
        }
    }
}