using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(IMediationService))]
    public sealed class MediationService : IMediationService
    {
        private readonly IDictionary<Type, object> _dictionary;
        
        public MediationService() => _dictionary = new Dictionary<Type, object>();
        
        public TModel Get<TModel>()
        {
            var type = typeof(TModel);
            if (_dictionary.TryGetValue(type, out var value))
                return (TModel) value;
            return default(TModel);
        }

        public void Set<TModel>(TModel model)
        {
            var type = model.GetType();
            _dictionary[type] = model;
        }
    }
}