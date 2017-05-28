using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace myFeed.Extensions.ViewModels
{
    /// <summary>
    /// Represents ComboBox view model.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public class ComboBoxViewModel<TKey, TValue> : 
        ViewModelBase, IUserSelectableProperty<TValue> 
        where TValue : IComparable
    {
        private object _selectedItem;

        /// <summary>
        /// Initializes a new instance of ComboBox ViewModel.
        /// </summary>
        public ComboBoxViewModel() => Items = new ObservableCollection<KeyValuePair<TKey, TValue>>();

        /// <summary>
        /// Initializes a new instance of ComboBox ViewModel.
        /// </summary>
        /// <param name="items">Collection of tuples that will represent ComboBox's k-v pairs.</param>
        /// <param name="index">Index of item that should be selected.</param>
        public ComboBoxViewModel(IEnumerable<KeyValuePair<TKey, TValue>> items, int index)
        {
            // Assert that selected index is less than items count.
            if (items.Count() <= index)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Fill the items collection.
            Items = new ObservableCollection<KeyValuePair<TKey, TValue>>();
            foreach (var item in items)
                Items.Add(item);

            // Select needed item.
            SelectedItem = Items[index];
        }

        /// <summary>
        /// ComboBox items.
        /// </summary>
        public ObservableCollection<KeyValuePair<TKey, TValue>> Items { get; }

        /// <summary>
        /// Selected item.
        /// </summary>
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (EqualityComparer<object>.Default.Equals(_selectedItem, value)) return;

                // Assign and raise property changed.
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                // Raise ValueChanged.
                SelectedValueChanged?.Invoke(this,
                    ((KeyValuePair<TKey, TValue>)SelectedItem).Value
                );
            }
        }

        /// <summary>
        /// Sets selected item by a known value, finds it in the collection.
        /// </summary>
        /// <param name="value">Known value</param>
        public void SetSelectedItem(TValue value) => SelectedItem = Items.First(i => i.Value.CompareTo(value) == 0);

        /// <summary>
        /// Invoked when user-selected value of binding ComboBox changes.
        /// </summary>
        public event EventHandler<TValue> SelectedValueChanged;
    }
}
