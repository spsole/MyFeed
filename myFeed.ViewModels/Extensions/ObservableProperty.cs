using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace myFeed.ViewModels.Extensions
{
    /// <summary>
    /// Observable property that provides an easy way 
    /// of setting and updating bindable values.
    /// </summary>
    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T _value;

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

    /// <summary>
    /// Observable Property extensions.
    /// </summary>
    public static class ObservableProperty
    {
        /// <summary>
        /// Creates new ObservableProperty of T.
        /// </summary>
        public static ObservableProperty<T> Of<T>() => new ObservableProperty<T>();
        
        /// <summary>
        /// Creates observable property from a value.
        /// </summary>
        public static ObservableProperty<T> Of<T>(T value) => new ObservableProperty<T> {Value = value};

        /// <summary>
        /// Creates observable property from function of task.
        /// </summary>
        public static ObservableProperty<T> Of<T>(Func<Task<T>> function)
        {
            var property = new ObservableProperty<T>();
            UpdateProperty(); 
            return property; 
            
            async void UpdateProperty() => property.Value = await function();
        }
    }
}