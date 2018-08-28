using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Services;
using MyFeed.Tests.Attributes;
using NSubstitute;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class LiteFavoriteManagerTests
    {
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IFavoriteManager _favoriteManager;

        public LiteFavoriteManagerTests() => _favoriteManager = new LiteFavoriteManager(
            _categoryManager, Environment.Database
        );
        
        [Theory]
        [InlineData("Foo")]
        [InlineData("Bar")]
        [CleanUpCollection(nameof(Article))]
        public async Task ShouldInsertFavoriteArticlesIntoDatabase(string title)
        {
            await _favoriteManager.Insert(new Article {Title = title});
            var response = await _favoriteManager.GetAll();
            var articles = new List<Article>(response);
            
            articles.Count.Should().Be(1);
            articles.First().Title.Should().Be(title);
            articles.First().Fave.Should().BeTrue();
        }

        [Fact]
        [CleanUpCollection(nameof(Article))]
        public async Task ShouldInitializeUniqueIdentitiesWhenInsertingArticles()
        {
            await _favoriteManager.Insert(new Article());
            await _favoriteManager.Insert(new Article());
            var response = await _favoriteManager.GetAll();
            var articles = new List<Article>(response);
            articles.First().Id.Should().NotBe(articles.Last().Id);
        }

        [Fact]
        [CleanUpCollection(nameof(Article))]
        public async Task ShouldBeAbleToRemoveArticlesFromTheCollection()
        {
            var article = new Article();
            await _favoriteManager.Insert(article);
            await _favoriteManager.Remove(article);
            var articles = await _favoriteManager.GetAll();
            articles.Count().Should().Be(0);
        }
    }
}