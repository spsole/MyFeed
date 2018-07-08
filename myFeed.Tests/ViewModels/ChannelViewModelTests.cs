using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using myFeed.ViewModels;
using NSubstitute;
using Xunit;

namespace myFeed.Tests.ViewModels
{
    public sealed class ChannelViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
        private readonly Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> _itemFactory;
        private readonly Func<Category, ChannelViewModel, ChannelGroupViewModel> _groupFactory;
        private readonly ChannelViewModel _channelViewModel;
        
        public ChannelViewModelTests()
        {
            _itemFactory = (x, y, z) => new ChannelItemViewModel(z, _categoryManager, _platformService, y, x); 
            _groupFactory = (x, y) => new ChannelGroupViewModel(_itemFactory, y, _categoryManager, x);
            _channelViewModel = new ChannelViewModel(_groupFactory, _navigationService, _categoryManager);
        }

        [Fact]
        public void ShouldInitializeChannelViewModelProperly()
        {
            _channelViewModel.Categories.Should().BeEmpty();
            _channelViewModel.IsLoading.Should().BeTrue();
            _channelViewModel.IsEmpty.Should().BeFalse();
        }

        [Fact]
        public void ShouldDisableAddCommandWhenCategoryNameIsEmpty()
        {
            var command = (ICommand) _channelViewModel.CreateCategory;
            command.CanExecute(null).Should().BeFalse();
            _channelViewModel.CategoryName = "Hello, world!";
            command.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public async Task ShouldBeMarkedAsEmptyIfThereIsNoCategoriesInStorage()
        {
            _categoryManager.GetAll().Returns(new List<Category>());
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(100);

            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.IsEmpty.Should().BeTrue();
            _channelViewModel.Categories.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldInitializeNestedGroupViewModelWithCategoryName()
        {
            var category = new Category {Title = "Foo", Channels = new List<Channel>()};
            _categoryManager.GetAll().Returns(new List<Category> {category});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);

            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.IsEmpty.Should().BeFalse();
            _channelViewModel.Categories.Should().NotBeEmpty();
            _channelViewModel.Categories.First().Title.Should().Be("Foo");
        }

        [Fact]
        public async Task ShouldRearrangeGroupsProperly()
        {
            var foo = new Category {Title = "Foo", Channels = new List<Channel>()};
            var bar = new Category {Title = "Bar", Channels = new List<Channel>()};
            _categoryManager.GetAll().Returns(new List<Category> {foo, bar});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);

            _channelViewModel.Categories.Count.Should().Be(2);
            _channelViewModel.Categories.First().Title.Should().Be("Foo");
            _channelViewModel.Categories.Last().Title.Should().Be("Bar");

            var last = _channelViewModel.Categories.Last();
            _channelViewModel.Categories.Remove(last);
            _channelViewModel.Categories.Insert(0, last);
            await Task.Delay(300);
            await _categoryManager.Received(1).Rearrange(
                Arg.Any<IEnumerable<Category>>()
            );
        }

        [Fact]
        public async Task ShouldRemoveCategoryFromDatabaseAndFromUi()
        {
            var category = new Category {Title = "Foo"};
            _categoryManager.GetAll().Returns(new List<Category> {category});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);
            
            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.Categories.Count.Should().Be(1);
            var group = _channelViewModel.Categories.First();
            group.Remove.Execute().Subscribe();
            await Task.Delay(300);

            await _categoryManager.Received(1).Remove(Arg.Any<Category>());
            _channelViewModel.Categories.Should().BeEmpty();
            _channelViewModel.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldAddChannelsToCategories()
        {
            var category = new Category {Title = "Foo"};
            _categoryManager.GetAll().Returns(new List<Category> {category});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);
            
            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.Categories.Count.Should().Be(1);
            var group = _channelViewModel.Categories.First();

            group.ChannelUri = "http://foo.bar";
            group.CreateChannel.Execute().Subscribe();
            await Task.Delay(100);
            
            group.Channels.Should().NotBeEmpty();
            group.Channels.First().Url.Should().Be("http://foo.bar");
            group.ChannelUri.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldUpdateCategoryInDatabaseOnceWhenTitlePropertyChanges()
        {
            var category = new Category {Title = "Foo", Channels = new List<Channel>()};
            _categoryManager.GetAll().Returns(new List<Category> {category});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);

            _channelViewModel.Categories.Should().NotBeEmpty();
            var groupViewModel = _channelViewModel.Categories.First();
            groupViewModel.RealTitle.Should().Be("Foo");
            groupViewModel.Title.Should().Be("Foo");
            groupViewModel.Title = "B";
            groupViewModel.Title = "Ba";
            groupViewModel.Title = "Bar";
            groupViewModel.RealTitle.Should().Be("Foo");
            await Task.Delay(1000);

            groupViewModel.RealTitle.Should().Be("Bar");
            groupViewModel.Title.Should().Be("Bar");
            category.Title.Should().Be("Bar");
            await _categoryManager.Received(1).Update(category);
        }

        [Fact]
        public async Task ShouldOpenAbsoluteUriHost()
        {
            var channel = new Channel {Uri = "http://vc.ru/feed"};
            var category = new Category {Channels = new List<Channel> {channel}};
            _categoryManager.GetAll().Returns(new List<Category> {category});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);

            _channelViewModel.Categories.Should().NotBeEmpty();
            _channelViewModel.Categories.First().Channels.Should().NotBeEmpty();
            var item = _channelViewModel.Categories.First().Channels.First();
            item.Open.Execute().Subscribe();

            await _platformService.Received(1).LaunchUri(Arg.Any<Uri>());
            _platformService
                .ReceivedCalls()
                .First()
                .GetArguments()
                .First()
                .As<Uri>()
                .ToString()
                .Should()
                .Be("http://vc.ru/");
        }
    }
}