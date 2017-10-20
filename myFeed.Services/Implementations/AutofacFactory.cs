using System.Linq;
using Autofac;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public class AutofacFactoryService : IFactoryService
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacFactoryService(ILifetimeScope lifetimeScope) => _lifetimeScope = lifetimeScope;
        
        public TObject CreateInstance<TObject>(params object[] arguments)
        {
            var parameters = arguments.Select(i => new TypedParameter(i.GetType(), i));
            return _lifetimeScope.Resolve<TObject>(parameters);
        }
    }
}