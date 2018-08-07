using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Services;
using myFeed.Tests.Attributes;
using Xunit;

namespace myFeed.Tests.Services
{
    public sealed class LiteCategoryManagerTests
    {
        private readonly ICategoryManager _categoryManager;

        public LiteCategoryManagerTests() => _categoryManager = new LiteCategoryManager(Environment.Database);

        [Theory]
        [InlineData("Foo")]
        [InlineData("LongerTitle")]
        [InlineData("Title with spaces")]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldInsertCategoriesIntoDatabase(string title)
        {
            await _categoryManager.Insert(new Category {Title = title});
            var categories = await _categoryManager.GetAll();
            categories.First().Title.Should().Be(title);
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldOrderCategoriesWhenInsertingOnes()
        {
            await _categoryManager.Insert(new Category {Title = "Foo"});
            await _categoryManager.Insert(new Category {Title = "Bar"});
            await _categoryManager.Insert(new Category {Title = "Zoo"});
            
            var response = await _categoryManager.GetAll();
            var categories = new List<Category>(response);
            categories[0].Order.Should().Be(0);
            categories[1].Order.Should().Be(1);
            categories[2].Order.Should().Be(2);
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldRemoveCategoriesFromDatabase()
        {
            var category = new Category();
            await _categoryManager.Insert(category);
            await _categoryManager.Remove(category);
            var all = await _categoryManager.GetAll();
            all.Count().Should().Be(0);
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldUpdateInsertedCategories()
        {
            var category = new Category {Title = "Foo"};
            await _categoryManager.Insert(category);
            category.Title = "Bar";
            
            await _categoryManager.Update(category);
            var response = await _categoryManager.GetAll();
            response.First().Title.Should().Be("Bar");
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldChangeCategoriesOrderBasedOnSequenceOrder()
        {
            var array = new List<Category>
            {
                new Category {Title = "One"},
                new Category {Title = "Two"},
                new Category {Title = "Three"}
            };
            foreach (var category in array)
            {
                await _categoryManager.Insert(category);
            }
            var rearranged = new[] {array[2], array[0], array[1]};
            await _categoryManager.Rearrange(rearranged);

            var response = await _categoryManager.GetAll();
            var categories = new List<Category>(response);
            categories[0].Title.Should().Be("Three");
            categories[1].Title.Should().Be("One");
            categories[2].Title.Should().Be("Two");
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldReturnNullIfNoArticlesExistForGivenId()
        {
            var article = await _categoryManager.GetArticleById(Guid.Empty);
            article.Should().BeNull();
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldFindConcreteNestedArticleUsingItsId()
        {
            var identity = Guid.NewGuid();
            await _categoryManager.Insert(new Category 
            {
                Channels = new List<Channel>
                {
                    new Channel
                    {
                        Articles = new List<Article>
                        {
                            new Article
                            {
                                Id = identity, 
                                Title = "Secret"
                            }
                        }
                    }
                }
            });
            
            var article = await _categoryManager.GetArticleById(identity);
            article.Title.Should().Be("Secret");
            article.Id.Should().Be(identity);
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldInsertChannelIntoGivenCategory()
        {
            var category = new Category();
            var channel = new Channel {Uri = "Foo"};
            await _categoryManager.Insert(category);
            
            category.Channels.Add(channel);
            await _categoryManager.Update(category);
            
            var response = await _categoryManager.GetAll();
            response.First().Channels.First().Uri.Should().Be("Foo");
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShoudUpdateChannelExistingInDatabase()
        {
            var channel = new Channel {Uri = "Foo"};
            var category = new Category {Channels = new List<Channel> {channel}};
            await _categoryManager.Insert(category);

            channel.Uri = "Bar";
            await _categoryManager.Update(channel);
            var response = await _categoryManager.GetAll();
            response.First().Channels.First().Uri.Should().Be("Bar");
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldUpdateExistingArticlesInDatabase()
        {
            var article = new Article {Title = "Foo"};
            await _categoryManager.Insert(new Category
            {
                Channels = new List<Channel>
                {
                    new Channel
                    {
                        Articles = new List<Article> {article}
                    }
                }
            });
            article.Title = "Bar";
            await _categoryManager.Update(article);
            var response = await _categoryManager.GetAll();
            response.First().Channels.First().Articles.First().Title.Should().Be("Bar");
        }

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public Task ShouldReturnFalseIfNoChannelExists() => Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _categoryManager.Update(new Channel())
        );

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public Task ShouldNotThrowIfNoArticleExists() => _categoryManager.Update(new Article());

        [Fact]
        [CleanUpCollection(nameof(Category))]
        public async Task ShouldNotHaveNullIdsOnNestedEntities()
        {
            await _categoryManager.Insert(new Category
            {
                Channels = new List<Channel>
                {
                    new Channel
                    {
                        Articles = new List<Article>
                        {
                            new Article()
                        }
                    }
                }
            });
            var response = await _categoryManager.GetAll();
            var categories = new List<Category>(response);
            categories.First().Id.Should().NotBe(Guid.Empty);
            categories.First().Channels.First().Id.Should().NotBe(Guid.Empty);
            categories.First().Channels.First().Articles.First().Id.Should().NotBe(Guid.Empty);
        }
    }
}