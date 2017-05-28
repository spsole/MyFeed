using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace myFeed.Extensions
{
    /// <summary>
    /// A deeply observable collection that watches for changes in it's items properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeeplyObservableCollection<T> : ObservableCollection<T>, IDisposable where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new DeeplyObservaleCollection instance.
        /// </summary>
        public DeeplyObservableCollection()
        {
            CollectionChanged += DeeplyObservableCollection_CollectionChanged;
        }

        /// <summary>
        /// Destructs a collection.
        /// </summary>
        ~DeeplyObservableCollection()
        {
            CollectionChanged -= DeeplyObservableCollection_CollectionChanged;
        }

        /// <summary>
        /// Dettaches unnessesary handles, attaches nessesary ones.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeeplyObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (T item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;

            if (e.OldItems != null)
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
        }

        /// <summary>
        /// Resets a collection when a property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        /// <summary>
        /// Disposes the entire collection.
        /// </summary>
        public virtual void Dispose()
        {
            CollectionChanged -= DeeplyObservableCollection_CollectionChanged;
            foreach (var item in Items)
                item.PropertyChanged -= Item_PropertyChanged;
            Clear();
            CollectionChanged += DeeplyObservableCollection_CollectionChanged;
        }

        #region Property changed helpers 

        /// <summary>
        /// Sets a property field in functional-like style.
        /// </summary>
        /// <typeparam name="TValue">Generic type</typeparam>
        /// <param name="field">Ref to hidden field to be set by a property</param>
        /// <param name="value">Value to set</param>
        /// <param name="selectorExpression">Selector expression that should return current property</param>
        protected void SetField<TValue>(ref TValue field, TValue value, Expression<Func<TValue>> selectorExpression)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(selectorExpression);
        }

        /// <summary>
        /// Raises property changed event.
        /// </summary>
        /// <typeparam name="TValue">Type</typeparam>
        /// <param name="selectorExpression">Selector expression</param>
        protected void OnPropertyChanged<TValue>(Expression<Func<TValue>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException(nameof(selectorExpression));

            var body = selectorExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));
        }

        #endregion
    }
}
