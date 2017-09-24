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
    public class Property<T> : INotifyPropertyChanged
    {
        private T _value;

        /// <summary>
        /// Initializes a new instance of observable property.
        /// </summary>
        public Property() : this(default(T)) {}

        /// <summary>
        /// Initializes observable property from value.
        /// </summary>
        public Property(T value) => _value = value;
    
        /// <summary>
        /// Initializes observable property from task.
        /// </summary>
        public Property(Func<Task<T>> function) => UpdateValue(function);

        /// <summary>
        /// Asynchroniously updates property value.
        /// </summary>
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