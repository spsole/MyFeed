﻿using System;
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
            var mediationServiceFactory = _resolver.Resolve<Func<object[], IStateContainer>>();
            var mediationService = mediationServiceFactory.Invoke(arguments);
            
            var resolutionFactory = _resolver.Resolve<Func<IStateContainer, TObject>>();
            return resolutionFactory.Invoke(mediationService);
        }
    }
}