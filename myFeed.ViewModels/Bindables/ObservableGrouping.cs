using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using myFeed.ViewModels.Implementations;

namespace myFeed.ViewModels.Bindables
{
    public class ObservableGrouping<TKey, TElement> : ObservableCollection<TElement>, IGrouping<TKey, TElement>
    {
        public ObservableGrouping(IGrouping<TKey, TElement> grouping) : base(grouping) => Key = grouping.Key;

        public ObservableGrouping(TKey key, IEnumerable<TElement> elements) : base(elements) => Key = key;

        public TKey Key { get; }
    }
}
