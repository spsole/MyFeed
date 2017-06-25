namespace myFeed.Extensions.Mvvm
{
    /// <summary>
    /// Selectable property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISelectableProperty<T> 
        : IObservableProperty<T>, ISelectable<T> { }
}
