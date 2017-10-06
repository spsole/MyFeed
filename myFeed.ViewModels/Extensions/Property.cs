using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace myFeed.ViewModels.Extensions
{
    public sealed class Property<TValue> : INotifyPropertyChanged
    {
        private TValue _encapsulatedField;

        public Property() : this(default(TValue)) { }

        public Property(TValue value) => _encapsulatedField = value;

        public Property(Func<Task<TValue>> function) => UpdateValue(function);

        public event PropertyChangedEventHandler PropertyChanged;

        public TValue Value
        {
            get => _encapsulatedField;
            set
            {
                if (EqualityComparer<TValue>.Default.Equals(
                    _encapsulatedField, value)) return;
                _encapsulatedField = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        private async void UpdateValue(Func<Task<TValue>> function) => Value = await function();
    }
}