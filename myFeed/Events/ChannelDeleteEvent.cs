using myFeed.Models;
using myFeed.ViewModels;

namespace myFeed.Events
{
    public sealed class ChannelDeleteEvent
    {
        public Channel Channel { get; }
        
        public ChannelItemViewModel ChannelItemViewModel { get; }
        
        public ChannelDeleteEvent(Channel channel, ChannelItemViewModel channelItemViewModel)
        {
            ChannelItemViewModel = channelItemViewModel;
            Channel = channel;
        }
    }
}