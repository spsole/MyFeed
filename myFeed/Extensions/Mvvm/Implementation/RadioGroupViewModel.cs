using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace myFeed.Extensions.Mvvm.Implementation
{
    /// <summary>
    /// Radio group view model.
    /// </summary>
    /// <typeparam name="T">Generic typedef</typeparam>
    public class RadioGroupViewModel<T> : ViewModelBase, ISelectableProperty<T> where T : IComparable
    {
        /// <summary>
        /// Initializes a new instance of ComboBox ViewModel.
        /// </summary>
        public RadioGroupViewModel() { }

        /// <summary>
        /// Initializes a new instance of RadioButtonGroup ViewModel.
        /// </summary>
        /// <param name="items">Collection of data that will be converted to radio button models.</param>
        /// <param name="index">Index of item that should be selected.</param>
        public RadioGroupViewModel(IEnumerable<T> items, int index)
        {
            // Assert arguments.
            var list = items.ToList();
            if (list.Count <= index)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Fill the collection.
            foreach (var item in list)
            {
                var model = new RadioButtonViewModel<T>(item);
                model.PropertyChanged += OnModelPropertyChanged;
                Items.Add(model);
            }
            
            // Select the item manually.
            Items[index].IsSelected = true;
        }

        /// <summary>
        /// Items of the radio group.
        /// </summary>
        public ObservableCollection<RadioButtonViewModel<T>> Items { get; } = 
            new ObservableCollection<RadioButtonViewModel<T>>();

        /// <summary>
        /// Invoked when Radio Button properties change.
        /// </summary>
        /// <param name="sender">Sender (RadioButton)</param>
        /// <param name="e">Args</param>
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var button = (RadioButtonViewModel<T>)sender;
            if (e.PropertyName == nameof(button.IsSelected) &&
                button.IsSelected == true)
            {
                ValueChanged?.Invoke(this, button.Data);
            }
        }

        /// <summary>
        /// Sets selected item by a known value, finds it in the collection.
        /// </summary>
        /// <param name="value">Known value</param>
        public void SetSelectedItem(T value)
        {
            Items.ToList().ForEach(i => i.IsSelected = false);
            Items.First(i => i.Data.CompareTo(value) >= 0).IsSelected = true;
        }

        /// <summary>
        /// Invoked when user-selected value of binding ComboBox changes.
        /// </summary>
        public event EventHandler<T> ValueChanged;
    }

    /// <summary>
    /// Radio button view model.
    /// </summary>
    /// <typeparam name="T">Generic typedef</typeparam>
    public class RadioButtonViewModel<T> : ViewModelBase
    {
        private T _data;
        private bool? _isSelected = false;

        /// <summary>
        /// Initializes a new instance of RadioButtonViewModel.
        /// </summary>
        /// <param name="value">Associated value.</param>
        public RadioButtonViewModel(T value) => Data = value;

        /// <summary>
        /// Is this button selected or not.
        /// </summary>
        public bool? IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value, () => IsSelected);
        }

        /// <summary>
        /// Contains data related to this radioButton.
        /// </summary>
        public T Data
        {
            get => _data;
            set => SetField(ref _data, value, () => Data);
        }
    }
}
