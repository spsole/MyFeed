namespace myFeed.ViewModels.Extensions {
    /// <summary>
    /// Represents read only property.
    /// </summary>
    public struct ReadOnlyProperty<T> {
        /// <summary>
        /// Inits new readonly property holding value.
        /// </summary>
        /// <param name="value">Value to hold.</param>
        public ReadOnlyProperty(T value) => Value = value;

        /// <summary>
        /// Read only property value.
        /// </summary>
        public T Value { get; }
    }
}
