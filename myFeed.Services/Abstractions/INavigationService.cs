using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Provides platform-specific navigation actions.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Brings user into ViewModel.
        /// </summary>
        /// <param name="viewModelType">ViewModel to show.</param>
        Task Navigate(Type viewModelType);

        /// <summary>
        /// Brings user into ViewModel with parameter.
        /// </summary>
        /// <param name="viewModelType">ViewModel to show.</param>
        /// <param name="parameter">Parameter to pass.</param>
        Task Navigate(Type viewModelType, object parameter);

        /// <summary>
        /// Invoked when view changes.
        /// </summary>
        event EventHandler<Type> Navigated;

        /// <summary>
        /// Returns menu icons for application main menu containing 
        /// platform-type-specific icon codes.
        /// </summary>
        IReadOnlyDictionary<Type, object> Icons { get; }
    }
}
