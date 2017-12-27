using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface INavigationService
    {
        Task Navigate<TViewModel>() where TViewModel : class;
        
        Task Navigate<TViewModel>(object parameter) where TViewModel : class;

        event EventHandler<Type> Navigated;

        IReadOnlyDictionary<Type, object> Icons { get; }
    }
}
