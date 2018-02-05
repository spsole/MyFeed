using System;
using System.Collections.Generic;
using DryIoc;
using DryIoc.MefAttributedModel;

namespace myFeed
{
    public static class Extensions 
    {
        public static void RegisterShared(this IContainer registrator)
        {
            registrator.WithMefAttributedModel();
            var assembly = typeof(Extensions).GetAssembly();
            registrator.RegisterExports(new [] {assembly});
        }
        
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));
            var index = 0;
            foreach (var element in source)
                action(element, index++);
        }
    }
}