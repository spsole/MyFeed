using System;
using System.ComponentModel.Composition;
using DryIoc;
using DryIocAttributes;
using myFeed.Services.Abstractions;

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
            var mediationServiceFactory = _resolver.Resolve<Func<object[], IStateContainer>>();
            var mediationService = mediationServiceFactory.Invoke(arguments);
            
            var resolutionFactory = _resolver.Resolve<Func<IStateContainer, TObject>>();
            return resolutionFactory.Invoke(mediationService);
        }
    }
}