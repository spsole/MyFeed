using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed class FeedViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly IFeedStoreService _feedStoreService = Substitute.For<IFeedStoreService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IFavoriteManager _favoriteManager = Substitute.For<IFavoriteManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();
        
        private readonly Func<FeedItemViewModel, FeedItemFullViewModel> _fullFactory;
        private readonly Func<Category, FeedGroupViewModel> _groupFactory;
        private readonly Func<Article, FeedItemViewModel> _itemFactory;
        private readonly FeedViewModel _feedViewModel;

        public FeedViewModelTests()
        {
            _fullFactory = x => new FeedItemFullViewModel(x, _settingManager);
            _itemFactory = x => new FeedItemViewModel(_fullFactory, _navigationService, _categoryManager, _favoriteManager, _platformService, x);
            _groupFactory = x => new FeedGroupViewModel(_itemFactory, _navigationService, _feedStoreService, _settingManager, x);
            _feedViewModel = new FeedViewModel(_groupFactory, _navigationService, _categoryManager, _settingManager);
        }

        [Fact]
        public void ShouldBeUninitializedAndLoadingWhenNotLoaded()
        {
            _feedViewModel.Selection.Should().BeNull();
            _feedViewModel.IsLoading.Should().BeTrue();
            _feedViewModel.IsEmpty.Should().BeFalse();
            _feedViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public void ShouldInitializeGroupAsLoadingAndEmptyUntilBeingFetched()
        {
            var groupViewModel = _groupFactory.Invoke(new Category {Title = "Foo"});
            groupViewModel.ShowRead.Should().BeTrue();
            groupViewModel.IsLoading.Should().BeTrue();
            groupViewModel.IsEmpty.Should().BeFalse();
            groupViewModel.Items.Should().BeEmpty();
            groupViewModel.Title.Should().Be("Foo");
        }
        
        [Fact]
        public async Task ShouldNotifyOfPropertyChange()
        {
            var triggered = false;
            var notifyPropertyChanged = (INotifyPropertyChanged)(object)_feedViewModel;
            notifyPropertyChanged.PropertyChanged += delegate { triggered = true; };
            
            _categoryManager.GetAll().Returns(new List<Category>());
            _settingManager.Read().Returns(new Settings());
            _feedViewModel.Load.Execute().Subscribe();
            
            await Task.Delay(100);
            triggered.Should().BeTrue();
        }
        
        [Fact]
        public async Task ShouldNotifyOfGroupPropertyChange()
        {
            var triggered = false;
            var groupViewModel = _groupFactory.Invoke(new Category {Title = "Foo"});
            var notifyPropertyChanged = (INotifyPropertyChanged)(object)groupViewModel;
            notifyPropertyChanged.PropertyChanged += delegate { triggered = true; };
            
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>());
            _settingManager.Read().Returns(new Settings());
            groupViewModel.Fetch.Execute().Subscribe();
            
            await Task.Delay(100);
            triggered.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldLoadCategoriesFromDiskIntoTheUi()
        {
            _settingManager.Read().Returns(new Settings());
            _categoryManager.GetAll().Returns(new List<Category> {new Category()});
            _feedViewModel.Load.Execute().Subscribe();
            await Task.Delay(100);

            _feedViewModel.IsEmpty.Should().BeFalse();
            _feedViewModel.IsLoading.Should().BeFalse();
            _feedViewModel.Items.Should().NotBeEmpty();
            await _categoryManager.Received(1).GetAll();
        }

        [Fact]
        public async Task ShouldReloadCategoriesFromDiskIntoTheUi()
        {
            _settingManager.Read().Returns(new Settings());
            _categoryManager.GetAll().Returns(new List<Category> {new Category()});
            _feedViewModel.Load.Execute().Subscribe();
            _feedViewModel.Load.Execute().Subscribe();
            await Task.Delay(100);
            
            _feedViewModel.IsEmpty.Should().BeFalse();
            _feedViewModel.IsLoading.Should().BeFalse();
            _feedViewModel.Items.Count.Should().Be(1);
        }

        [Fact]
        public async Task ShouldLoadSettingsAndMarkersProperly()
        {
            _settingManager.Read().Returns(new Settings {Images = true});
            _categoryManager.GetAll().Returns(new List<Category>());
            _feedViewModel.Load.Execute().Subscribe();
            await Task.Delay(100);
            
            _feedViewModel.IsEmpty.Should().BeTrue();
            _feedViewModel.Images.Should().BeTrue();
            _feedViewModel.IsLoading.Should().BeFalse();
            _feedViewModel.Items.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldLoadSettingsIntoTheUi()
        {
            var groupViewModel = _groupFactory.Invoke(new Category {Title = "Foo"});
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>());
            _settingManager.Read().Returns(new Settings {Read = false});
            groupViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            groupViewModel.IsLoading.Should().BeFalse();
            groupViewModel.IsEmpty.Should().BeTrue();
            groupViewModel.Items.Should().BeEmpty();
            groupViewModel.ShowRead.Should().BeFalse();
        }
    }
}