using System;

namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// Property that can be user-selected.
    /// </summary>
    public interface ISelectable<T>
    {
        /// <summary>
        /// Invoked when selected value changes.
        /// </summary>
        event EventHandler<T> ValueChanged;
    }
}
