using System.ComponentModel;

namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// Observable property schema.
    /// </summary>
    /// <typeparam name="TValue">Value type.</typeparam>
    public interface IObservableProperty<TValue> : INotifyPropertyChanged
    {
        /// <summary>
        /// Attached value.
        /// </summary>
        TValue Value { get; set; }
    }
}
