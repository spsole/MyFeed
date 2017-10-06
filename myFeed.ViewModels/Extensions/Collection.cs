using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace myFeed.ViewModels.Extensions
{
    public sealed class Collection<T> : ObservableCollection<T>
    {
        private const string CountName = nameof(Count);
        private const string IndexerName = "Item[]";
        
        public void AddRange(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentException(nameof(enumerable));

            CheckReentrancy();
            var collection = enumerable.ToList();
            foreach (var item in collection) Items.Add(item);
            
            OnPropertyChanged(new PropertyChangedEventArgs(CountName));
            OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}