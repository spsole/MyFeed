using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using myFeed.Services;
using NSubstitute;
using Xunit;

namespace myFeed.Tests.Services
{
    public sealed class BackgroundServiceTests
    {
        private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
        private readonly IFeedStoreService _feedStoreService = Substitute.For<IFeedStoreService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();
        private readonly IBackgroundService _backgroundService;

        public BackgroundServiceTests() => _backgroundService = new BackgroundService(
            _notificationService, _feedStoreService, _categoryManager, _settingManager
        );
        
        [Fact]
        public async Task ShouldSendOrderedNotificationsForArticlesWithGreaterDate()
        {
            IEnumerable<Article> received = null;
            _notificationService
                .When(x => x.SendNotifications(Arg.Any<IEnumerable<Article>>()))
                .Do(callback => received = callback.Arg<IEnumerable<Article>>());
            
            _settingManager.Read().Returns(new Settings {Fetched = DateTime.MinValue});
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>
            {
                new Article {Title = "Foo", PublishedDate = DateTime.Now},
                new Article {Title = "Bar", PublishedDate = DateTime.MaxValue}
            });

            await _backgroundService.CheckForUpdates(DateTime.Now);
            var response = new List<Article>(received);
            
            response.First().Title.Should().Be("Foo");
            response.Last().Title.Should().Be("Bar");
            response.Count.Should().Be(2);
        }

        [Fact]
        public async Task ShouldNotSendNotificationsForOutdatedArticles()
        {
            IEnumerable<Article> received = null;
            _notificationService
                .When(x => x.SendNotifications(Arg.Any<IEnumerable<Article>>()))
                .Do(callback => received = callback.Arg<IEnumerable<Article>>());
            
            _settingManager.Read().Returns(new Settings {Fetched = DateTime.Now});
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>
            {
                new Article {Title = "Foo", PublishedDate = DateTime.MinValue},
                new Article {Title = "Bar", PublishedDate = DateTime.MaxValue}
            });
            
            await _backgroundService.CheckForUpdates(DateTime.Now);
            var response = new List<Article>(received);
            
            response.First().Title.Should().Be("Bar");
            response.Count.Should().Be(1);
        }
    }
}