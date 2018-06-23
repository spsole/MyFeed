using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Func<Channel, Category, ChannelGroupViewModel, ChannelItemViewModel> _itemFactory;
        private readonly Func<Category, ChannelViewModel, ChannelGroupViewModel> _groupFactory;
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
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
            _channelViewModel.IsLoading.Should().BeTrue();
            _channelViewModel.IsEmpty.Should().BeFalse();
            _channelViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldBeMarkedAsEmptyIfThereIsNoCategoriesInStorage()
        {
            _categoryManager.GetAll().Returns(new List<Category>());
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(100);

            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.IsEmpty.Should().BeTrue();
            _channelViewModel.Items.Should().BeEmpty();
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
            _channelViewModel.Items.Should().NotBeEmpty();
            _channelViewModel.Items.First().Title.Should().Be("Foo");
        }

        [Fact]
        public async Task ShouldRearrangeGroupsProperly()
        {
            var foo = new Category {Title = "Foo", Channels = new List<Channel>()};
            var bar = new Category {Title = "Bar", Channels = new List<Channel>()};
            _categoryManager.GetAll().Returns(new List<Category> {foo, bar});
            _channelViewModel.Load.Execute().Subscribe();
            await Task.Delay(300);

            _channelViewModel.IsLoading.Should().BeFalse();
            _channelViewModel.IsEmpty.Should().BeFalse();
            _channelViewModel.Items.Count.Should().Be(2);
            _channelViewModel.Items.First().Title.Should().Be("Foo");
            _channelViewModel.Items.Last().Title.Should().Be("Bar");

            _channelViewModel.Items.Move(0, 1);
            await Task.Delay(100);
            await _categoryManager.Received(1).Rearrange(
                Arg.Any<IEnumerable<Category>>()
            );
        }
    }
}