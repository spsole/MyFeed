using System;

namespace myFeed.Extensions.ViewModels
{
    /// <summary>
    /// Represents 
    /// </summary>
    public interface IUserSelectableProperty<T>
    {
        /// <summary>
        /// Invoked when user-selected value changes.
        /// </summary>
        event EventHandler<T> SelectedValueChanged;
    }
}
