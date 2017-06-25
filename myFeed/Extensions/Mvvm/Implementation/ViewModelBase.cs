using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace myFeed.Extensions.Mvvm.Implementation
{
    /// <summary>
    /// View model's base class that can react on internal changes.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this when some property changes.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Sets a property field in functional-like style.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="field">Ref to hidden field to be set by a property</param>
        /// <param name="value">Value to set</param>
        /// <param name="selectorExpression">Selector expression that should return current property</param>
        protected void SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            RaisePropertyChanged(selectorExpression);
        }

        /// <summary>
        /// Raises property changed event.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="selectorExpression">Selector expression</param>
        private void RaisePropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException(nameof(selectorExpression));
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(body.Member.Name);
        }
    }
}
