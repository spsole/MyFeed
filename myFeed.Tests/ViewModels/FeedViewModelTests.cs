using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed class FeedViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();
        private readonly Func<Category, FeedGroupViewModel> _factory = _ => null;
        private readonly FeedViewModel _feedViewModel;

        public FeedViewModelTests() => _feedViewModel = new FeedViewModel(
            _factory, _navigationService, _categoryManager, _settingManager
        );

        [Fact]
        public void ShouldBeUninitializedAndLoadingWhenNotLoaded()
        {
            _feedViewModel.Selection.Should().BeNull();
            _feedViewModel.IsLoading.Should().BeTrue();
            _feedViewModel.IsEmpty.Should().BeFalse();
            _feedViewModel.Items.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldNotifyOfPropertyChange()
        {
            _categoryManager.GetAll().Returns(new List<Category>());
            _settingManager.Read().Returns(new Settings());

            var triggered = false;
            var notifyPropertyChanged = (INotifyPropertyChanged)(object)_feedViewModel;
            notifyPropertyChanged.PropertyChanged += delegate { triggered = true; };
            _feedViewModel.Load.Execute().Subscribe();
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
    }
}