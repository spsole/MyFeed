using System;
using DryIoc;
using DryIocAttributes;
using myFeed.Interfaces;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IFactoryService))]
    public sealed class DryIocFactoryService : IFactoryService
    {
        private readonly IResolver _resolver;

        public DryIocFactoryService(IResolver resolver) => _resolver = resolver;

        public TFactory Create<TFactory>() => _resolver.Resolve<TFactory>();
    }
}