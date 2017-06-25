using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// A deeply observable collection that watches for changes in it's items properties.
    /// </summary>
    public interface IDeeplyObservableCollection<T>
        : INotifyPropertyChanged, INotifyCollectionChanged, ICollection<T> where T 
        : INotifyPropertyChanged { }
}
