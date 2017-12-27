using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIocAttributes;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(IStateContainer))]
    public sealed class DryIocStateContainer : IStateContainer
    {
        private readonly IList<object> _state;
        
        public DryIocStateContainer(object[] state) => _state = state.ToList();
        
        public TModel Pop<TModel>()
        {
            var type = typeof(TModel);
            var model = _state.FirstOrDefault(i => i.GetType() == type);
            if (model == null) return default(TModel);
            _state.Remove(model);
            return (TModel) model;
        }
    }
}