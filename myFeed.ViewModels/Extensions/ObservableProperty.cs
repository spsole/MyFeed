using System.Collections.Generic;
using System.ComponentModel;

namespace myFeed.ViewModels.Extensions
{
    /// <summary>
    /// Observable property that provides an easy way 
    /// of setting and updating values.
    /// </summary>
    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T _value;

        /// <summary>
        /// Initializes a new instance of observable property.
        /// </summary>
        public ObservableProperty() => _value = default(T);

        /// <summary>
        /// Initializes a new instance of observable property
        /// with custom default value.
        /// </summary>
        /// <param name="value">Default value.</param>
        public ObservableProperty(T value) => _value = value;

        /// <summary>
        /// Invoked when property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Value of the ObservableProperty instance.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }
}