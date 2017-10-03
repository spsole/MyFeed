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
        Task Navigate<T>() where T : class;

        /// <summary>
        /// Brings user into ViewModel with custom ViewModel.
        /// </summary>
        Task Navigate<T>(T parameter) where T : class;

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
