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
    public sealed class SearchViewModelTests
    {
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
        private readonly ISearchService _searchService = Substitute.For<ISearchService>();
        
        private readonly Func<FeedlyItem, SearchItemViewModel> _factory;
        private readonly SearchViewModel _searchViewModel;

        public SearchViewModelTests()
        {
            _factory = x => new SearchItemViewModel(_categoryManager, _platformService, x);
            _searchViewModel = new SearchViewModel(_factory, _searchService);
        }

        [Fact]
        public void ShouldBeEmptyLoadingAndGreetingWhenInitialized()
        {
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsGreeting.Should().BeTrue();
            _searchViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public void ShouldInitializeChildViewModelFromModel()
        {
            var searchItemViewModel = _factory.Invoke(new FeedlyItem {Description = "Bar", Title = "Foo"});
            searchItemViewModel.Description.Should().Be("Bar");
            searchItemViewModel.Title.Should().Be("Foo");
        }
        
        [Fact]
        public async Task ShouldNotifyOfPropertyChange()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem>()};
            _searchService.Search("q").Returns(response);
            
            var triggered = false;
            var notifyPropertyChanged = (INotifyPropertyChanged)(object)_searchViewModel;
            notifyPropertyChanged.PropertyChanged += delegate { triggered = true; };
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            triggered.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldLoadItemsReceivedFromSearchService()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem> {new FeedlyItem()}};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.Items.Count.Should().Be(1);
            await _searchService.Received().Search("q");
        }

        [Fact]
        public async Task ShouldCorrectlyIndicateIfNothingIsFound()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem>()};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Fetch.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeTrue();
            _searchViewModel.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldTriggerFetchCommandWhenSearchQueryChanges()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem> {new FeedlyItem()}};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            await Task.Delay(1000);    
            
            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.Items.Count.Should().Be(1);
        }
    }
}