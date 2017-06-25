using System.Collections.ObjectModel;
using myFeed.Extensions.Mvvm.Implementation;

namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// Radio group view model.
    /// </summary>
    /// <typeparam name="T">Generic typedef</typeparam>
    public interface IRadioGroupViewModel<T> : ISelectable<T>
    {
        /// <summary>
        /// Items of the radio group.
        /// </summary>
        ObservableCollection<RadioButtonViewModel<T>> Items { get; }

        /// <summary>
        /// Selects an item.
        /// </summary>
        /// <param name="item">Item to select.</param>
        void Select(T item);
    }

    /// <summary>
    /// Radio button view model.
    /// </summary>
    /// <typeparam name="T">Generic typedef</typeparam>
    public interface IRadioButtonViewModel<out T>
    {
        /// <summary>
        /// Is this button selected or not.
        /// </summary>
        IObservableProperty<bool> IsSelected { get; }

        /// <summary>
        /// Contains data related to this radioButton.
        /// </summary>
        T Value { get; }
    }
}
