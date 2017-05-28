using System;

namespace myFeed.Extensions.ViewModels
{
    /// <summary>
    /// Represents selectable property view model. 
    /// For example, it can be used for ToggleSwitch controls.
    /// </summary>
    /// <typeparam name="TValue">Property type</typeparam>
    public class SelectableProperty<TValue> : ObservableProperty<TValue>, 
        IUserSelectableProperty<TValue> where TValue : IComparable
    {
        /// <summary>
        /// Initializes a new instance of selectable property.
        /// </summary>
        public SelectableProperty() { }

        /// <summary>
        /// Initializes a new instance of selectable property.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        public SelectableProperty(TValue defaultValue) : base(defaultValue) { }

        /// <summary>
        /// Value of the SelectableProperty instance.
        /// </summary>
        public override TValue Value
        {
            get => _value;
            set
            {
                // Return if equals.
                if (value.CompareTo(_value) == 0) return;
                
                // Update field and invoke methods.
                SetField(ref _value, value, () => Value);
                SelectedValueChanged?.Invoke(this, Value);
            }
        }

        /// <summary>
        /// Sets selected item as a known value.
        /// </summary>
        /// <param name="value">Value.</param>
        public void SetSelectedItem(TValue value) => Value = value;

        /// <summary>
        /// Invoked when user-selected value of binding ComboBox changes.
        /// </summary>
        public event EventHandler<TValue> SelectedValueChanged;
    }
}
