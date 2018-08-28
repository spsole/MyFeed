using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Services;
using NSubstitute;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class ParallelFeedStoreTests
    {
        private readonly IFeedFetchService _feedFetchService = Substitute.For<IFeedFetchService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();
        private readonly IFeedStoreService _feedStoreService;

        public ParallelFeedStoreTests() => _feedStoreService = new ParallelFeedStoreService(
            _feedFetchService, _categoryManager, _settingManager
        );
        
        [Fact]
        public async Task ShouldSortStoredArticleEntities()
        {
            _settingManager.Read().Returns(new Settings {Max = 5});
            _feedFetchService.Fetch(Arg.Any<string>()).Returns(new List<Article>());
            var channel = new Channel {Articles = new List<Article>
            {
                new Article {Title = "Foo", PublishedDate = DateTime.Now},
                new Article {Title = "Bar", PublishedDate = DateTime.MinValue},
                new Article {Title = "Zoo", PublishedDate = DateTime.MaxValue}
            }};
            
            var result = await _feedStoreService.Load(new List<Channel> {channel});
            var articles = new List<Article>(result);

            articles.Count.Should().Be(3);
            articles[0].Title.Should().Be("Zoo");
            articles[1].Title.Should().Be("Foo");
            articles[2].Title.Should().Be("Bar");
        }

        [Fact]
        public async Task ShouldSaveFetchedArticleEntities()
        {
            _settingManager.Read().Returns(new Settings {Max = 5});
            _feedFetchService.Fetch(Arg.Any<string>()).Returns(new List<Article>{new Article {Title = "Foo"}});
            var channels = new List<Channel> {new Channel {Uri = "http://foo.bar"}};

            IEnumerable<Article> inserted = null;
            _categoryManager
                .When(x => x.Update(Arg.Any<Channel>()))
                .Do(callback => inserted = callback.Arg<Channel>().Articles);
            
            var result = await _feedStoreService.Load(channels);

            var articlesInserted = new List<Article>(inserted);
            articlesInserted[0].Title.Should().Be("Foo");
            articlesInserted.Count.Should().Be(1);
            
            var articlesLoaded = new List<Article>(result);
            articlesLoaded[0].Title.Should().Be("Foo");
            articlesLoaded.Count.Should().Be(1);
        }

        [Fact]
        public async Task ShouldOrderFetchedAndStoredArticlesByDate()
        {
            var fetchedArticles = new List<Article> {new Article {Title = "Foo", PublishedDate = DateTime.Now}};
            _feedFetchService.Fetch(Arg.Any<string>()).Returns(fetchedArticles);
            _settingManager.Read().Returns(new Settings {Max = 5});
            
            var result = await _feedStoreService.Load(new List<Channel> {new Channel
            {
                Articles = new List<Article> {new Article {Title = "Bar", PublishedDate = DateTime.MinValue}}
            }});
            
            var articles = new List<Article>(result);
            articles[0].Title.Should().Be("Foo");
            articles[1].Title.Should().Be("Bar");
            articles.Count.Should().Be(2);
        }

        [Fact]
        public async Task ShouldRemoveOutdatedArticlesIfCountIsCreaterThenLimit()
        {
            _settingManager.Read().Returns(new Settings {Max = 70});
            _feedFetchService.Fetch(Arg.Any<string>()).Returns(new List<Article>());
            var result = await _feedStoreService.Load(new List<Channel> {new Channel
            {
                Articles = Enumerable.Repeat(new Article(), 200).ToList()
            }});
            var articles = new List<Article>(result);
            articles.Count.Should().Be(70);
        }

        [Fact]
        public async Task ShouldIgnoreWhitespaceWhenComparingArticles()
        {
            _settingManager.Read().Returns(new Settings {Max = 5});
            _feedFetchService.Fetch(Arg.Any<string>()).Returns(new List<Article>
            {
                new Article {Title = " Foo   ", FeedTitle = "Bar"},
                new Article {Title = "Bar\r\n", FeedTitle = "Foo"}
            });
            var result = await _feedStoreService.Load(new List<Channel> {new Channel {Articles = new List<Article>
            {
                new Article {Title = "Foo", FeedTitle = "Bar"},
                new Article {Title = "Bar", FeedTitle = "Foo"}
            }}});

            var articles = new List<Article>(result);
            articles.Count.Should().Be(2);
            articles.Last().Title.Should().Be("Bar");
            articles.First().Title.Should().Be("Foo");
            articles.First().FeedTitle.Should().Be("Bar");
            articles.Last().FeedTitle.Should().Be("Foo");
        }
    }
}