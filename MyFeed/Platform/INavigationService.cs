using System;
using System.Threading.Tasks;

namespace MyFeed.Platform
{
    public interface INavigationService
    {
        Task Navigate<TViewModel>();
        
        Task NavigateTo<TViewModel>(TViewModel viewModel);

        IObservable<Type> Navigated { get; }
        
        Type CurrentViewModelType { get; }
    }
}
