using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using MyFeed.ViewModels;
using NSubstitute;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class FaveViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IFavoriteManager _favoriteManager = Substitute.For<IFavoriteManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
        private readonly ISettingManager _settingManager = Substitute.For<ISettingManager>();

        private readonly Func<IGrouping<string, Article>, FaveGroupViewModel> _groupFactory;
        private readonly Func<FeedItemViewModel, FeedItemFullViewModel> _fullItemFactory;
        private readonly Func<Article, FeedItemViewModel> _itemFactory;
        private readonly FaveViewModel _faveViewModel;

        public FaveViewModelTests()
        {
            _fullItemFactory = x => new FeedItemFullViewModel(x, _settingManager);
            _itemFactory = x => new FeedItemViewModel(_fullItemFactory, _navigationService, _categoryManager, _favoriteManager, _platformService, x);
            _groupFactory = x => new FaveGroupViewModel(_itemFactory, x);
            _faveViewModel = new FaveViewModel(_groupFactory, _navigationService, _favoriteManager, _settingManager);
        }

        [Fact]
        public void ShouldBeUninitializedAndLoadingWhenNotLoaded()
        {
            _faveViewModel.IsLoading.Should().BeTrue();
            _faveViewModel.IsEmpty.Should().BeFalse();
            _faveViewModel.Images.Should().BeFalse();
            _faveViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldLoadFavoriteItemsInGroupsFromDisk()
        {
            var article = new Article { Title = "Foo", FeedTitle = "Bar" };
            _favoriteManager.GetAll().Returns(new[] {article});
            _settingManager.Read().Returns(new Settings());
            _faveViewModel.Load.Execute().Subscribe();
            await Task.Delay(500);

            _faveViewModel.IsLoading.Should().BeFalse();
            _faveViewModel.IsEmpty.Should().BeFalse();
            _faveViewModel.Items.Count.Should().Be(1);
        }

        [Fact]
        public async Task ShouldBeMarkedAsLoadedEmptyAndGreetingIfThereAreNoItems()
        {
            _favoriteManager.GetAll().Returns(new Article[0]);
            _settingManager.Read().Returns(new Settings());
            _faveViewModel.Load.Execute().Subscribe();
            await Task.Delay(500);

            _faveViewModel.IsLoading.Should().BeFalse();
            _faveViewModel.IsEmpty.Should().BeTrue();
            _faveViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldLoadSettingsFromSettingsManager()
        {
            _favoriteManager.GetAll().Returns(new Article[0]);
            _settingManager.Read().Returns(new Settings {Images = true});
            _faveViewModel.Load.Execute().Subscribe();
            await Task.Delay(500);

            _faveViewModel.IsLoading.Should().BeFalse();
            _faveViewModel.Images.Should().BeTrue();
        }
    }
}