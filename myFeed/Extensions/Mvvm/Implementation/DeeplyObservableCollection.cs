using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace myFeed.Extensions.Mvvm.Implementation
{
    /// <summary>
    /// A deeply observable collection that watches for changes in it's items properties.
    /// </summary>
    /// <typeparam name="T">Items types.</typeparam>
    public class DeeplyObservableCollection<T> 
        : ObservableCollection<T>, IDeeplyObservableCollection<T> where T 
        : INotifyPropertyChanged
    {
        /// <summary>
        /// Inits an empty deep-watchable collection.
        /// </summary>
        public DeeplyObservableCollection() { }

        /// <summary>
        /// Inits a new DeeplyObservableCollection from a seq.
        /// </summary>
        /// <param name="enumerable">Seq.</param>
        public DeeplyObservableCollection(IEnumerable<T> enumerable) : base(enumerable) { }

        /// <summary>
        /// Dettaches unnessesary handlers, attaches nessesary ones.
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (T item in e.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;
            if (e.OldItems != null)
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Resets a collection when a property changes.
        /// </summary>
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var itemSender = (T)sender;
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    itemSender,
                    itemSender,
                    IndexOf(itemSender)
                )
            );
        }
    }
}
