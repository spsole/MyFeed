using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface INavigationService
    {
        Task Navigate<TViewModel>() where TViewModel : class;

        Task Navigate<TViewModel>(object parameter) where TViewModel : class;

        IReadOnlyDictionary<Type, (string, object)> Icons { get; }

        IObservable<Type> Navigated { get; }
    }
}
