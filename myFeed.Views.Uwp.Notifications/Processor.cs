using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Notifications
{
    internal sealed class Processor
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IFeedService _feedService;

        public Processor(IFeedService feedService, 
            IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _feedService = feedService;
        }

        public Task ProcessFeeds() => Task.CompletedTask;
    }
}
