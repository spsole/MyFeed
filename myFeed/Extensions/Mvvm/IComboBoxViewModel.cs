using System.Collections.ObjectModel;

namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// Represents ComboBox view model.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public interface IComboBoxViewModel<TKey, TValue> : ISelectableProperty<IComboBoxItemViewModel<TKey, TValue>>
    {
        /// <summary>
        /// ComboBox Items.
        /// </summary>
        ObservableCollection<IComboBoxItemViewModel<TKey, TValue>> Items { get; }
    }

    /// <summary>
    /// ComboBox item.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public interface IComboBoxItemViewModel<out TKey, out TValue>
    {
        /// <summary>
        /// Key.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Value.
        /// </summary>
        TValue Value { get; }
    }
}
