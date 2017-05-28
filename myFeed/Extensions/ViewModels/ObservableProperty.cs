namespace myFeed.Extensions.ViewModels
{
    /// <summary>
    /// Observable property that provides an easy way 
    /// of setting and updating it's value.
    /// </summary>
    public class ObservableProperty<TValue> : ViewModelBase
    {
        protected TValue _value;

        /// <summary>
        /// Initializes a new instance of observable property.
        /// </summary>
        public ObservableProperty() => _value = default(TValue);

        /// <summary>
        /// Initializes a new instance of observable property.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        public ObservableProperty(TValue defaultValue) => _value = defaultValue;

        /// <summary>
        /// Value of the ObservableProperty instance.
        /// </summary>
        public virtual TValue Value
        {
            get => _value;
            set => SetField(ref _value, value, () => Value);
        }
    }
}
