namespace myFeed.Tests.ViewModels

open System
open System.Linq
open System.Threading.Tasks
open System.Collections.Generic

open NSubstitute
open Autofac
open Xunit

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dep

open myFeed.ViewModels
open myFeed.ViewModels.Implementations
open myFeed.ViewModels.Extensions

open myFeed.Services.Abstractions
open myFeed.Services.Implementations

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Implementations

open myFeed.Entities
open myFeed.Entities.Feedly
open myFeed.Entities.Local

/// Tests for menu ViewModel.
type MenuViewModelFixture() =

    [<Fact>]
    member x.``should load menu viewmodel``() =
        let navigationService = Substitute.For<INavigationService>()
        navigationService.Icons.Returns(dict
            [ typeof<FaveViewModel>, null;
              typeof<FeedViewModel>, null
              typeof<SearchViewModel>, null;
              typeof<SettingsViewModel>, null;
              typeof<SourcesViewModel>, null ] 
            |> Dictionary<_, _>) |> ignore   
        let menuViewModel =
            MenuViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                navigationService,
                Substitute.For<_>())
        Assert.Equal(0, menuViewModel.SelectedIndex.Value)
        Assert.NotEmpty(menuViewModel.Items)

// Tests for search ViewModel.
type SearchViewModelFixture() =

    [<Fact>]
    member x.``should inject model instance into search item viewmodel``() =    
        let searchItemViewModel = 
            SearchItemViewModel(
                SearchItemEntity(Title="Foo"),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>())
        Should.equal "Foo" searchItemViewModel.Title.Value

    [<Fact>]
    member x.``should insert items received from search service``() =
        let searchService = Substitute.For<ISearchService>()
        searchService.Search(Arg.Any()).Returns(
            SearchRootEntity(
                Results=([ SearchItemEntity(Title="Foo");
                   SearchItemEntity(Title="Bar") ] |> List<_>))) |> ignore

        let searchViewModel =
            SearchViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                searchService)

        searchViewModel.Fetch.CanExecuteChanged += fun _ ->
            if (searchViewModel.Fetch.CanExecute()) then

                Should.equal "Foo" searchViewModel.Items.[0].Title.Value
                Should.equal "Bar" searchViewModel.Items.[1].Title.Value
                Should.equal false searchViewModel.IsEmpty.Value
                Should.equal false searchViewModel.IsLoading.Value
                
        searchViewModel.Fetch.Execute(null)

    [<Fact>]
    member x.``should add feed url to sources and encapsulate properties``() =
        let searchEntity = SearchItemEntity(Title="Bar", FeedId="_____http://example.com")
        let fakeEntity = SourceCategoryEntity(Title="Foo")
            
        let dialogService = Substitute.For<IDialogService>()
        dialogService.ShowDialogForSelection(Arg.Any())
            .Returns(fakeEntity :> obj |> Task.FromResult) |> ignore

        let searchItemViewModel =
            SearchItemViewModel(
                searchEntity,
                dialogService,
                Substitute.For<_>(),
                Substitute.For<_>())
        
        searchItemViewModel.AddToSources.CanExecuteChanged += fun _ -> 
            if (searchItemViewModel.AddToSources.CanExecute()) then

                Should.equal "Bar" searchItemViewModel.Title.Value
                Should.equal "http://example.com" searchItemViewModel.FeedUrl.Value

        searchItemViewModel.AddToSources.Execute(null)            

type SettingsViewModelFixture() =
    let fakeSettingsService =
        let settings = Substitute.For<ISettingsService>()
        settings.Get<_>(Arg.Is("Theme")).Returns("Foo" |> Task.FromResult) |> ignore
        settings.Get<_>(Arg.Is("LoadImages")).Returns(true |> Task.FromResult) |> ignore
        settings.Get<_>(Arg.Is("NeedBanners")).Returns(true |> Task.FromResult) |> ignore
        settings.Get<_>(Arg.Is("NotifyPeriod")).Returns(42 |> Task.FromResult) |> ignore
        settings.Get<_>(Arg.Is("FontSize")).Returns(42 |> Task.FromResult) |> ignore
        settings.Set(Arg.Any(), Arg.Any()).Returns(Task.CompletedTask) |> ignore
        settings
    
    [<Fact>]
    member x.``should load data properly from repository``() =
        let settingsViewModel =
            SettingsViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                fakeSettingsService,
                Substitute.For<_>(),
                Substitute.For<_>())

        settingsViewModel.Load.CanExecuteChanged += fun _ ->
            if (settingsViewModel.Load.CanExecute()) then

                Assert.Equal(42, settingsViewModel.FontSize.Value)
                Assert.Equal("Foo", settingsViewModel.Theme.Value)
                Assert.Equal(true, settingsViewModel.NeedBanners.Value)

        settingsViewModel.Load.Execute(null)

    [<Fact>]
    member x.``should watch for changes of property values``() =
        let mutable count = 0
        let platformService = Substitute.For<IPlatformService>()
        platformService.RegisterTheme(Arg.Any()).Returns(Task.CompletedTask)
            .AndDoes(fun _ -> count <- count + 1) |> ignore
        platformService.RegisterBackgroundTask(Arg.Any()).Returns(Task.CompletedTask)
            .AndDoes(fun i -> count <- count + 1) |> ignore

        let settingsViewModel = 
            SettingsViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                fakeSettingsService,
                platformService,
                Substitute.For<_>())        

        settingsViewModel.Load.CanExecuteChanged += fun _ ->
            if (settingsViewModel.Load.CanExecute()) then   
                
                settingsViewModel.Theme.Value <- "Bar"
                settingsViewModel.NeedBanners.Value <- false
                settingsViewModel.NotifyPeriod.Value <- 0
                Assert.Equal(false, settingsViewModel.NeedBanners.Value)
                Assert.Equal(0, settingsViewModel.NotifyPeriod.Value)
                Assert.Equal("Bar", settingsViewModel.Theme.Value)
                Assert.Equal(2, count)

        settingsViewModel.Load.Execute(null)             

type FeedViewModelsTests() =

    [<Fact>]
    member x.``should populate feed category with received articles``() =
        let sourceCategoryEntity = SourceCategoryEntity(Title="Foo", Sources=
            [| SourceEntity(Uri="http://example.com") |])

        let feedStoreService = Substitute.For<IFeedStoreService>()
        feedStoreService.GetAsync(Arg.Any()).Returns(
            struct(null, 
                [ ArticleEntity(Title="Foo");
                  ArticleEntity(Title="Bar") ]
                |> fun sqs -> sqs.OrderBy(fun i -> i.Title)) 
            |> Task.FromResult) |> ignore   

        let feedViewModel = 
            FeedCategoryViewModel(
                sourceCategoryEntity,
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                feedStoreService,
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>())        

        feedViewModel.Fetch.CanExecuteChanged += fun _ ->
            if (feedViewModel.Fetch.CanExecute()) then
                
                Assert.Equal(2, feedViewModel.Items.Count)
                Assert.Equal("Bar", feedViewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", feedViewModel.Items.[1].Title.Value)
                Assert.Equal(false, feedViewModel.IsLoading.Value)
                Assert.Equal(false, feedViewModel.IsEmpty.Value)
                Assert.Equal("Foo", feedViewModel.Title.Value)

        feedViewModel.Fetch.Execute(null)  

    [<Fact>]
    member x.``should populate feed viewmodel with items from db``() =
        let sourcesRepository = Substitute.For<ISourcesRepository>()
        sourcesRepository.GetAllAsync().Returns(
            [ SourceCategoryEntity(Title="Foo", Sources=
                [| SourceEntity(Uri="http://example.com") |]);
              SourceCategoryEntity(Title="Bar") ]
            |> fun sqs -> sqs.OrderBy(fun i -> i.Title)
            |> Task.FromResult) 
            |> ignore

        let feedViewModel =
            FeedViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                sourcesRepository,
                Substitute.For<_>(),
                Substitute.For<_>())

        feedViewModel.Load.CanExecuteChanged += fun _ ->
            if (feedViewModel.Load.CanExecute()) then 
                
                Assert.Equal(2, feedViewModel.Items.Count)
                Assert.Equal("Bar", feedViewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", feedViewModel.Items.[1].Title.Value)

        feedViewModel.Load.Execute(null)                   

type FaveViewModelFixture() =

    [<Fact>]
    member x.``should populate fave viewmodel with items from repo``() =
        let articlesRepository = Substitute.For<IArticlesRepository>()
        articlesRepository.GetAllAsync().Returns(
            [ ArticleEntity(Title="Foo", Fave=true);
              ArticleEntity(Title="Bar", Fave=true) ] 
            :> seq<_> |> Task.FromResult) |> ignore

        let faveViewModel = 
            FaveViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                articlesRepository,
                Substitute.For<_>())

        faveViewModel.Load.CanExecuteChanged += fun _ ->
            if (faveViewModel.Load.CanExecute()) then

                Assert.Equal(false, faveViewModel.IsLoading.Value)
                Assert.Equal(false, faveViewModel.IsEmpty.Value)
                Assert.Equal("Foo", faveViewModel.Items.[0].Title.Value)
                Assert.Equal("Bar", faveViewModel.Items.[1].Title.Value)

        faveViewModel.Load.Execute(null)

    [<Fact>]
    member x.``should mark article as read and as unread``() = 
        let articleEntity = ArticleEntity(Read=false)
        let articleViewModel =
            ArticleViewModel(
                articleEntity,
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>())

        articleViewModel.MarkRead.Execute()  
        Assert.Equal(true, articleEntity.Read)         

        articleViewModel.MarkRead.Execute(null)
        Assert.Equal(false, articleEntity.Read)

    [<Fact>]
    member x.``should mark article as favorite``() =
        let articleEntity = ArticleEntity(Fave=false)
        let articleViewModel = 
            ArticleViewModel(
                articleEntity,
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>())

        articleViewModel.MarkFavorite.Execute(null)
        Assert.Equal(true, articleEntity.Fave)

type SourcesViewModelsTests() =

    [<Fact>]
    member x.``child viewmodels should load properly``() =        
        let sourcesRepository = Substitute.For<ISourcesRepository>()
        sourcesRepository.GetAllAsync().Returns(
            [ SourceCategoryEntity(Title="Foo");
              SourceCategoryEntity(Title="Bar") ]
            |> fun sqs -> sqs.OrderBy(fun i -> i.Title)
            |> Task.FromResult) |> ignore

        let sourcesViewModel = 
            SourcesViewModel(
                Substitute.For<_>(),
                Substitute.For<_>(),
                sourcesRepository,
                Substitute.For<_>(),
                Substitute.For<_>())

        sourcesViewModel.Load.CanExecuteChanged += fun _ ->
            if sourcesViewModel.Load.CanExecute() then

                Assert.Equal(false, sourcesViewModel.IsLoading.Value)
                Assert.Equal(false, sourcesViewModel.IsEmpty.Value)
                Assert.Equal(2, sourcesViewModel.Items.Count)
                Assert.Equal("Bar", sourcesViewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", sourcesViewModel.Items.[1].Title.Value)

        sourcesViewModel.Load.Execute(null)

    [<Fact>]
    member x.``should inject entity into sources category ViewModel``() =
        let sourceCategoryEntity = SourceCategoryEntity(Title="Foo")
        let sourceCategoryViewModel = 
            SourcesCategoryViewModel(
                sourceCategoryEntity, null,
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>(),
                Substitute.For<_>())
        Assert.Equal("Foo", sourceCategoryViewModel.Title.Value)

    [<Fact>]
    member x.``should create instance of sources item viewmodel``() =
        let sourceEntity = SourceEntity(Notify=true, Uri="https://google.com")
        let sourceItemViewModel =
            SourcesItemViewModel(
                sourceEntity, null,
                Substitute.For<_>(),
                Substitute.For<_>())
        Assert.Equal("https://google.com", sourceItemViewModel.Url.Value)
        Assert.Equal(true, sourceItemViewModel.Notify.Value)

    [<Fact>]
    member x.``should add entity to selected source category entity via viewmodel``() =
        let mutable counter = 0
        let sourceCategoryEntity = SourceCategoryEntity(Title="Foo")
        let sourcesRepository = Substitute.For<ISourcesRepository>()
        sourcesRepository.AddSourceAsync(Arg.Any(), Arg.Any())
            .Returns(Task.CompletedTask).AndDoes(fun _ -> counter <- counter + 1) 
            |> ignore

        let sourcesCategoryViewModel =
            SourcesCategoryViewModel(
                sourceCategoryEntity, null,
                Substitute.For<_>(),
                Substitute.For<_>(),
                sourcesRepository,
                Substitute.For<_>())
                
        sourcesCategoryViewModel.SourceUri.Value <- "http://foo.bar"
        sourcesCategoryViewModel.AddSource.Execute()

        Should.equal 1 sourcesCategoryViewModel.Items.Count
        Should.equal String.Empty sourcesCategoryViewModel.SourceUri.Value
        Should.equal "http://foo.bar" sourcesCategoryViewModel.Items.[0].Url.Value
        Should.equal 1 counter
