using myFeed.Models;
using myFeed.ViewModels;

namespace myFeed.Events
{
    public sealed class CategoryDeleteEvent
    {
        public Category Category { get; }
        
        public ChannelGroupViewModel ChannelGroupViewModel { get; }
        
        public CategoryDeleteEvent(Category category, ChannelGroupViewModel channelGroupViewModel)
        {
            ChannelGroupViewModel = channelGroupViewModel;
            Category = category;
        }
    }
}