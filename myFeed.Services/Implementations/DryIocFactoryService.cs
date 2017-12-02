using System;
using System.ComponentModel.Composition;
using myFeed.Services.Abstractions;
using DryIoc;
using DryIocAttributes;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFactoryService))]
    public sealed class DryIocFactoryService : IFactoryService
    {
        private readonly IResolver _resolver;

        public DryIocFactoryService(IResolver resolver) => _resolver = resolver;
        
        public TObject CreateInstance<TObject>(params object[] arguments)
        {
            var mediationService = _resolver.Resolve<IMediationService>();
            foreach (var argument in arguments) 
                mediationService.Set(argument);
            
            var resolution = _resolver.Resolve<Func<IMediationService, TObject>>();
            return resolution.Invoke(mediationService);
        }
    }
}