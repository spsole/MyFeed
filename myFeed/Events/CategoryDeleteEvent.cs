using myFeed.Models;
using myFeed.ViewModels;

namespace myFeed.Events
{
    public sealed class CategoryDeleteEvent
    {
        public Category Model { get; }
        
        public ChannelGroupViewModel ViewModel { get; }
        
        public CategoryDeleteEvent(Category model, ChannelGroupViewModel viewModel)
        {
            ViewModel = viewModel;
            Model = model;
        }
    }
}