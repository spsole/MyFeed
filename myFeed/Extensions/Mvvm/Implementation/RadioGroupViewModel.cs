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
    public class RadioGroupViewModel<T> : ViewModelBase, IRadioGroupViewModel<T>
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
                model.IsSelected.PropertyChanged += OnModelPropertyChanged;
                Items.Add(model);
            }
            
            // Select the item manually.
            Items[index].IsSelected.Value = true;
        }

        /// <summary>
        /// Items of the radio group.
        /// </summary>
        public ObservableCollection<RadioButtonViewModel<T>> Items { get; } = 
            new ObservableCollection<RadioButtonViewModel<T>>();

        /// <summary>
        /// Selects an item.
        /// </summary>
        /// <param name="item">Item to select.</param>
        public void Select(T item)
        {
            foreach (var button in Items)
                button.IsSelected.Value = false;
            Items.First(i => Equals(i.Value, item)).IsSelected.Value = true;
        }

        /// <summary>
        /// Invoked when Radio Button properties change.
        /// </summary>
        /// <param name="sender">Sender (RadioButton)</param>
        /// <param name="e">Args</param>
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var isSelected = (IObservableProperty<bool>)sender;
            if (isSelected.Value)
                ValueChanged?.Invoke(this, Items.First(
                    i => i.IsSelected.Value).Value);
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
    public class RadioButtonViewModel<T> : ViewModelBase, IRadioButtonViewModel<T>
    {
        /// <summary>
        /// Initializes a new instance of RadioButtonViewModel.
        /// </summary>
        /// <param name="value">Associated value.</param>
        public RadioButtonViewModel(T value) => Value = value;

        /// <summary>
        /// Is this button selected or not.
        /// </summary>
        public IObservableProperty<bool> IsSelected { get; } =
            new ObservableProperty<bool>(false);

        /// <summary>
        /// Contains data related to this radioButton.
        /// </summary>
        public T Value { get; }
    }
}
