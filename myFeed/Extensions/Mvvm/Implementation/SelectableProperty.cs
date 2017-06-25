using System;
using System.Collections.Generic;

namespace myFeed.Extensions.Mvvm.Implementation
{
    /// <summary>
    /// Represents selectable property view model. 
    /// For example, it can be used for ToggleSwitch controls.
    /// </summary>
    /// <typeparam name="TValue">Property type</typeparam>
    public class SelectableProperty<TValue> : ViewModelBase, ISelectableProperty<TValue> 
    {
        private TValue _value;

        /// <summary>
        /// Initializes a new instance of selectable property.
        /// </summary>
        public SelectableProperty() { }

        /// <summary>
        /// Initializes a new instance of selectable property.
        /// </summary>
        /// <param name="value">Default value.</param>
        public SelectableProperty(TValue value) => _value = value;

        /// <summary>
        /// Value of the SelectableProperty instance.
        /// </summary>
        public TValue Value
        {
            get => _value;
            set => SetValue(value);
        }

        /// <summary>
        /// Updates selectable property value.
        /// </summary>
        /// <param name="value">Value.</param>
        private void SetValue(TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(_value, value)) return;
            SetField(ref _value, value, () => Value);
            ValueChanged?.Invoke(this, value);
        }

        /// <summary>
        /// Invoked when user-selected value of binding ComboBox changes.
        /// </summary>
        public event EventHandler<TValue> ValueChanged;
    }
}
