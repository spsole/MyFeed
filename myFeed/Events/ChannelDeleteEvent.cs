using myFeed.Models;
using myFeed.ViewModels;

namespace myFeed.Events
{
    public sealed class ChannelDeleteEvent
    {
        public Channel Model { get; }
        
        public ChannelItemViewModel ViewModel { get; }
        
        public ChannelDeleteEvent(Channel model, ChannelItemViewModel viewModel)
        {
            ViewModel = viewModel;
            Model = model;
        }
    }
}