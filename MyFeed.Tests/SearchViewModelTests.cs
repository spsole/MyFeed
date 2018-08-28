using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using MyFeed.ViewModels;
using NSubstitute;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class SearchViewModelTests
    {
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IPlatformService _platformService = Substitute.For<IPlatformService>();
        private readonly ISearchService _searchService = Substitute.For<ISearchService>();
        private readonly Func<FeedlyItem, SearchItemViewModel> _factory;
        private readonly SearchViewModel _searchViewModel;

        public SearchViewModelTests()
        {
            _factory = x => new SearchItemViewModel(_platformService, x);
            _searchViewModel = new SearchViewModel(_factory, _navigationService, _categoryManager, _searchService);
        }

        [Fact]
        public void ShouldBeEmptyLoadingAndGreetingWhenInitialized()
        {
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsGreeting.Should().BeTrue();
            _searchViewModel.Categories.Should().BeEmpty();
            _searchViewModel.Feeds.Should().BeEmpty();
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
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            triggered.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldLoadItemsReceivedFromSearchService()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem> {new FeedlyItem()}};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.Feeds.Count.Should().Be(1);
            await _searchService.Received().Search("q");
        }

        [Fact]
        public async Task ShouldCorrectlyIndicateIfNothingIsFound()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem>()};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeTrue();
            _searchViewModel.Feeds.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldTriggerFetchCommandWhenSearchQueryChanges()
        {
            var response = new FeedlyRoot {Results = new List<FeedlyItem> {new FeedlyItem()}};
            _searchService.Search("q").Returns(response);
            _searchViewModel.SearchQuery = "q";
            await Task.Delay(1100);    
            
            _searchViewModel.IsGreeting.Should().BeFalse();
            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.IsEmpty.Should().BeFalse();
            _searchViewModel.Feeds.Count.Should().Be(1);
        }

        [Fact]
        public async Task ShouldLoadCategoriesFromCategoryManager()
        {
            var response = new[] {new Category {Title = "Foo"}};
            _categoryManager.GetAll().Returns(response);
            _searchViewModel.RefreshCategories.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.Categories.Should().NotBeEmpty();
            _searchViewModel.Categories.First().Title.Should().Be("Foo");
        }

        [Fact]
        public async Task ShouldNotAllowAddingFeedsUntilFeedAndCategoryAreSelected()
        {
            _searchViewModel.SelectedCategory = null;
            _searchViewModel.SelectedFeed = null;
            await Task.Delay(100);

            var command = (ICommand)_searchViewModel.Add;
            command.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public async Task ShouldTryToAddFeedWhenInputIsCompletelyValid()
        {
            var handled = false;
            var item = new FeedlyItem {FeedId = "feed/http://google.com"};
            var categories = new List<Category> {new Category {Title = "Foo"}};
            var response = new FeedlyRoot {Results = new List<FeedlyItem> {item}};
            _categoryManager.GetAll().Returns(categories);
            _searchService.Search("q").Returns(response);

            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Added.RegisterHandler(handle => handle.SetOutput(handled = true));
            _searchViewModel.RefreshCategories.Execute().Subscribe();
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.Feeds.Should().NotBeEmpty();

            _searchViewModel.SelectedCategory = _searchViewModel.Categories.First();
            _searchViewModel.SelectedFeed = _searchViewModel.Feeds.First();
            _searchViewModel.Add.Execute().Subscribe();
            await _categoryManager.Received(1).Update(Arg.Any<Category>());
            handled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldSetIsChangedItemPropertyToTrueWhenSelectionChanges()
        {
            var items = new List<FeedlyItem> {new FeedlyItem(), new FeedlyItem()};
            _searchService.Search("q").Returns(new FeedlyRoot {Results = items});
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.Feeds.Count.Should().Be(2);
            _searchViewModel.SelectedFeed = _searchViewModel.Feeds[1];            
            _searchViewModel.Feeds[0].IsSelected.Should().BeFalse();           
            _searchViewModel.Feeds[1].IsSelected.Should().BeTrue();
        }
        
        [Fact]
        public async Task ShouldSetIsChangedItemPropertyToFalseWhenSelectionBecomesNull()
        {
            var items = new List<FeedlyItem> {new FeedlyItem(), new FeedlyItem()};
            _searchService.Search("q").Returns(new FeedlyRoot {Results = items});
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.IsLoading.Should().BeFalse();
            _searchViewModel.Feeds.Count.Should().Be(2);
            
            _searchViewModel.SelectedFeed = _searchViewModel.Feeds[0];            
            _searchViewModel.Feeds[0].IsSelected.Should().BeTrue();           
            _searchViewModel.Feeds[1].IsSelected.Should().BeFalse();

            _searchViewModel.SelectedFeed = null;         
            _searchViewModel.Feeds[0].IsSelected.Should().BeFalse();           
            _searchViewModel.Feeds[1].IsSelected.Should().BeFalse();

            _searchViewModel.SelectedFeed = _searchViewModel.Feeds[1];
            _searchViewModel.Feeds[0].IsSelected.Should().BeFalse();
            _searchViewModel.Feeds[1].IsSelected.Should().BeTrue();
        }
        
        [Fact]
        public async Task ShouldTriggerCopyInteractionWhenCopyingIsRequested()
        {
            var handled = false;
            var searchItemViewModel = _factory(new FeedlyItem());
            searchItemViewModel.Copied.RegisterHandler(handler => handler.SetOutput(handled = true));
            searchItemViewModel.Copy.Execute().Subscribe();
            await Task.Delay(100);
            handled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldDeselectItemWhenSearchCommandBeginsExecuting()
        {
            var results = new List<FeedlyItem> {new FeedlyItem()};
            _searchService.Search("q").Returns(new FeedlyRoot {Results = results});
            _searchViewModel.SearchQuery = "q";
            _searchViewModel.Search.Execute().Subscribe();
            await Task.Delay(100);

            _searchViewModel.Feeds.Should().NotBeEmpty();
            _searchViewModel.SelectedFeed = _searchViewModel.Feeds[0];  
            _searchViewModel.Search.Execute().Subscribe();
            _searchViewModel.SelectedFeed.Should().BeNull();
        }

        [Fact]
        public async Task ShouldMarkViewModelAsErroredWhenExceptionIsThrownInFetchCommand()
        {
            var called = 0;
            _searchViewModel.SearchQuery = "q";
            _searchService.Search("q").Returns(new FeedlyRoot {Results = new List<FeedlyItem>()});
            _searchService.When(x => x.Search("q"))
                          .Do(x => { if (called++ == 0) throw new Exception(); });

            _searchViewModel.HasErrors.Should().BeFalse();
            try { _searchViewModel.Search.Execute().Subscribe(); } catch { }

            await Task.Delay(300);
            _searchViewModel.HasErrors.Should().BeTrue("command terminates with exception");
            _searchViewModel.Search.Execute().Subscribe();

            await Task.Delay(300);
            _searchViewModel.HasErrors.Should().BeFalse("command executes normally");
        }
    }
}