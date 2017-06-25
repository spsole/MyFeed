using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace myFeed.Extensions.Mvvm.Implementation
{
    /// <summary>
    /// Represents ComboBox view model.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public class ComboBoxViewModel<TKey, TValue> : ViewModelBase, IComboBoxViewModel<TKey, TValue>
    {
        private IComboBoxItemViewModel<TKey, TValue> _value;

        /// <summary>
        /// Initializes a new instance of ComboBox ViewModel.
        /// </summary>
        public ComboBoxViewModel() { }

        /// <summary>
        /// Initializes a new instance of ComboBox ViewModel.
        /// </summary>
        /// <param name="items">Collection of tuples that will represent ComboBox's k-v pairs.</param>
        /// <param name="index">Index of item that should be selected.</param>
        public ComboBoxViewModel(IEnumerable<KeyValuePair<TKey, TValue>> items, int index)
        {
            var keyValuePairs = items as IList<KeyValuePair<TKey, TValue>> ?? items.ToList();
            if (keyValuePairs.Count <= index)
                throw new ArgumentOutOfRangeException(nameof(index));

            foreach (var item in keyValuePairs)
                Items.Add(new ComboBoxItemViewModel<TKey, TValue>(
                    item.Key, item.Value));

            Value = Items[index];
        }

        /// <summary>
        /// ComboBox items.
        /// </summary>
        public ObservableCollection<IComboBoxItemViewModel<TKey, TValue>> Items { get; } =
            new ObservableCollection<IComboBoxItemViewModel<TKey, TValue>>();

        /// <summary>
        /// Value of the SelectableProperty instance.
        /// </summary>
        public IComboBoxItemViewModel<TKey, TValue> Value
        {
            get => _value;
            set => SetValue(value);
        }

        /// <summary>
        /// Invoked when selected value changes.
        /// </summary>
        public event EventHandler<IComboBoxItemViewModel<TKey, TValue>> ValueChanged;

        /// <summary>
        /// Updates selectable property value.
        /// </summary>
        /// <param name="value">Value.</param>
        private void SetValue(IComboBoxItemViewModel<TKey, TValue> value)
        {
            if (EqualityComparer<IComboBoxItemViewModel<TKey, TValue>>.Default.Equals(_value, value)) return;
            SetField(ref _value, value, () => Value);
            RaiseValueChanged(value);
        }

        /// <summary>
        /// Raises ValueChanged event.
        /// </summary>
        /// <param name="value">New value.</param>
        private void RaiseValueChanged(IComboBoxItemViewModel<TKey, TValue> value) => ValueChanged?.Invoke(this, value);
    }

    /// <summary>
    /// ComboBox item.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public class ComboBoxItemViewModel<TKey, TValue> : IComboBoxItemViewModel<TKey, TValue>
    {
        public ComboBoxItemViewModel(TKey key, TValue value) => (Key, Value) = (key, value);

        /// <summary>
        /// Key.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Value.
        /// </summary>
        public TValue Value { get; }
    }
}
