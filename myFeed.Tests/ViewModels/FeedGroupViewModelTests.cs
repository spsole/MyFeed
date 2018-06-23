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
    public sealed class FeedGroupViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly IFeedStoreService _feedStoreService = Substitute.For<IFeedStoreService>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();
        private readonly Func<Article, FeedItemViewModel> _factory = _ => null;
        private readonly FeedGroupViewModel _feedGroupViewModel;
        private readonly Category _category = new Category();

        public FeedGroupViewModelTests() => _feedGroupViewModel = new FeedGroupViewModel(
            _factory, _navigationService, _feedStoreService, _settingManager, _category
        );

        [Fact]
        public void ShouldBeInitializedWithDefaultsUntilBeingFetched()
        {
            _feedGroupViewModel.ShowRead.Should().BeTrue();
            _feedGroupViewModel.IsLoading.Should().BeTrue();
            _feedGroupViewModel.IsEmpty.Should().BeFalse();
            _feedGroupViewModel.Items.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldNotifyOfPropertyChange()
        {
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>());
            _settingManager.Read().Returns(new Settings());

            var triggered = false;
            var notifyPropertyChanged = (INotifyPropertyChanged)(object)_feedGroupViewModel;
            notifyPropertyChanged.PropertyChanged += delegate { triggered = true; };
            _feedGroupViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            triggered.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldLoadSettingsIntoTheUi()
        {
            _feedStoreService.Load(Arg.Any<IEnumerable<Channel>>()).Returns(new List<Article>());
            _settingManager.Read().Returns(new Settings {Read = true});
            _feedGroupViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            _feedGroupViewModel.IsLoading.Should().BeFalse();
            _feedGroupViewModel.IsEmpty.Should().BeTrue();
            _feedGroupViewModel.Items.Should().BeEmpty();
            _feedGroupViewModel.ShowRead.Should().BeTrue();
        }
    }
}