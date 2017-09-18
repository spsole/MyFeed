using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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
        public ObservableProperty() : this(default(T)) {}

        /// <summary>
        /// Initializes a new instance of observable property
        /// with custom default value.
        /// </summary>
        /// <param name="value">Default value.</param>
        public ObservableProperty(T value) => _value = value;
    
        /// <summary>
        /// Initializes ObservableProperty from task result.
        /// </summary>
        /// <param name="function">Function returning value.</param>
        public ObservableProperty(Func<Task<T>> function) => UpdateValue(function);

        /// <summary>
        /// Asynchroniously updates property value.
        /// </summary>
        /// <param name="function">Function to invoke to get task to await.</param>
        private async void UpdateValue(Func<Task<T>> function) => Value = await function();

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