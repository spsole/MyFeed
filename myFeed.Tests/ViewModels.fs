namespace myFeed.Tests.ViewModels

open System
open System.Linq
open System.Threading.Tasks
open System.Collections.Generic

open Autofac
open Xunit
open Moq

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dep

open myFeed.ViewModels
open myFeed.ViewModels.Implementations
open myFeed.ViewModels.Extensions

open myFeed.Services.Abstractions
open myFeed.Services.Implementations

open myFeed.Repositories.Abstractions

open myFeed.Entities
open myFeed.Entities.Feedly
open myFeed.Entities.Local

[<AutoOpen>]
module ViewModelsHelpers =

    /// Registers default empty mocks.
    let registerDefaults (builder: ContainerBuilder) =
        builder
        |> also registerMock<IDefaultsService>
        |> also registerMock<INavigationService>
        |> also registerMock<ISettingsService>
        |> also registerMock<IDialogService>
        |> also registerMock<IFilePickerService>
        |> also registerMock<IPlatformService>
        |> also registerMock<ISourcesRepository>
        |> also registerMock<ITranslationsService>
        |> also registerMock<ISearchService>
        |> also registerMock<IArticlesRepository>
        |> also registerMock<IFeedService>
        |> ignore

// Tests for menu ViewModels.
module MenuViewModelsTests =

    [<Fact>]
    let ``should load menu viewmodel``() =
        let icons = 
            dict[ ViewKey.ArticleView, null;
                  ViewKey.FaveView, null;
                  ViewKey.FeedView, null
                  ViewKey.SearchView, null;
                  ViewKey.SettingsView, null;
                  ViewKey.SourcesView, null ] 
            |> Dictionary<_, _>
        let mock = Mock<INavigationService>()
        mock.SetupGet(fun i -> i.Icons)
            .Returns(icons) |> ignore
        let viewModel = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<MenuViewModel>
            |> also (registerInstanceAs<INavigationService> mock.Object)
            |> buildScope
            |> resolveOnce<MenuViewModel>
        Assert.Equal(0, viewModel.SelectedIndex.Value)
        Assert.NotEmpty(viewModel.Items)

// Tests for search ViewModel.
module SearchViewModelsTests =

    [<Fact>]
    let ``should create instance of search viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<SearchViewModel>
        |> buildScope
        |> also Should.resolve<SearchViewModel>
        |> dispose

    [<Fact>]
    let ``should create instance of search item viewmodel``() =    
        let entity = SearchItemEntity(Title="Foo")
        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<SearchItemViewModel>
            |> also (registerInstanceAs<SearchItemEntity> <| entity)
            |> buildScope

        let searchItemViewModel = resolve<SearchItemViewModel> scope
        Assert.NotNull(searchItemViewModel)
        Assert.Equal("Foo", searchItemViewModel.Title.Value)

    [<Fact>]
    let ``should insert items received from search service``() =
        let response = 
            let results = 
                [ SearchItemEntity(Title="Foo");
                  SearchItemEntity(Title="Bar") ]
            SearchRootEntity(Results=(results |> collection))
            |> Task.FromResult

        let mockSearchService = new Mock<ISearchService>()
        mockSearchService
            .Setup(fun i -> i.Search(It.IsAny<string>()))
            .Returns(response) |> ignore

        let searchViewModel =
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<SearchViewModel>
            |> also registerAsSelf<SearchItemViewModel>
            |> also (registerInstanceAs<ISearchService> mockSearchService.Object)
            |> buildScope
            |> resolveOnce<SearchViewModel>

        searchViewModel.Fetch.CanExecuteChanged += fun _ ->
            if (searchViewModel.Fetch.CanExecute()) then

                Assert.Equal("Foo", searchViewModel.Items.[0].Title.Value)
                Assert.Equal("Bar", searchViewModel.Items.[1].Title.Value)
                Assert.Equal(false, searchViewModel.IsEmpty.Value)
                Assert.Equal(false, searchViewModel.IsLoading.Value)
                
        searchViewModel.Fetch.Execute(null)

    [<Fact>]
    let ``should add feed url to sources``() =
        let searchEntity = SearchItemEntity(FeedId="_____http://example.com")
        let fakeEntity = SourceCategoryEntity(Title="Foo")
            
        let mockService = Mock<IDialogService>()
        mockService
            .Setup(fun i -> i.ShowDialogForSelection(It.IsAny<seq<obj>>()))
            .Returns(fakeEntity :> obj |> Task.FromResult) |> ignore

        let mockRepository = Mock<ISourcesRepository>()
        mockRepository
            .Setup(fun i -> i.AddSourceAsync(It.IsAny(), It.IsAny())) 
            .Callback<SourceCategoryEntity, SourceEntity>(fun c e -> 
                Assert.Equal("http://example.com", e.Uri)) |> ignore               

        let viewModel = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<SearchItemViewModel>
            |> also (registerInstanceAs<SearchItemEntity> searchEntity)
            |> also (registerInstanceAs<IDialogService> mockService.Object)
            |> buildScope
            |> resolveOnce<SearchItemViewModel>
        viewModel.AddToSources.Execute(null)            

// Tests for settings ViewModel.
module SettingsViewModelsTests =

    let registerSettingsDefaults (builder: ContainerBuilder) =
        builder
        |> also registerMock<ISettingsService>
        |> also registerMock<IPlatformService>
        |> also registerMock<IOpmlService>
        |> also registerMock<IDialogService>
        |> also registerMock<ITranslationsService>
        |> also registerAsSelf<SettingsViewModel>
        |> ignore

    let fakeSettingsService =
        let mock = Mock<ISettingsService>()
        mock.Setup(fun i -> i.Get<_>(It.IsIn<string>("Theme")))
            .Returns("Foo" |> Task.FromResult) |> ignore
        mock.Setup(fun i -> i.Get<_>(It.IsIn<string>("LoadImages", "NeedBanners")))
            .Returns(true |> Task.FromResult) |> ignore
        mock.Setup(fun i -> i.Get<_>(It.IsIn<string>("FontSize", "NotifyPeriod")))
            .Returns(42 |> Task.FromResult) |> ignore   
        mock.Setup(fun i -> i.Set(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask) |> ignore        
        mock.Object
    
    [<Fact>]
    let ``should create instance of settings viewmodel``() =
        ContainerBuilder()
        |> also registerSettingsDefaults 
        |> buildScope            
        |> also Should.resolve<SettingsViewModel>
        |> dispose

    [<Fact>]
    let ``should load data properly from repository``() =
        let viewModel = 
            ContainerBuilder()    
            |> also registerSettingsDefaults
            |> also (registerInstanceAs<ISettingsService> fakeSettingsService)
            |> buildScope
            |> resolveOnce<SettingsViewModel>
        viewModel.Load.CanExecuteChanged += fun _ ->
            if (viewModel.Load.CanExecute()) then

                Assert.Equal(42, viewModel.FontSize.Value)
                Assert.Equal("Foo", viewModel.Theme.Value)
                Assert.Equal(true, viewModel.NeedBanners.Value)

        viewModel.Load.Execute(null)

    [<Fact>]
    let ``should watch for changes of property values``() =
        let mutable count = 0
        let mock = Mock<IPlatformService>()
        mock.Setup(fun i -> i.RegisterTheme(It.IsAny()))
            .Returns(Task.CompletedTask)
            .Callback(fun i -> count <- count + 1) |> ignore
        mock.Setup(fun i -> i.RegisterBackgroundTask(It.IsAny()))
            .Returns(Task.CompletedTask) 
            .Callback(fun i -> count <- count + 1) |> ignore

        let viewModel = 
            ContainerBuilder()
            |> also registerSettingsDefaults
            |> also (registerInstanceAs<ISettingsService> fakeSettingsService)
            |> also (registerInstanceAs<IPlatformService> mock.Object)
            |> buildScope
            |> resolveOnce<SettingsViewModel>
        viewModel.Load.CanExecuteChanged += fun _ ->
            if (viewModel.Load.CanExecute()) then   
                
                Assert.Equal("Foo", viewModel.Theme.Value)
                viewModel.Theme.Value <- "Bar"
                viewModel.NeedBanners.Value <- false
                viewModel.NotifyPeriod.Value <- 0
                Assert.Equal(false, viewModel.NeedBanners.Value)
                Assert.Equal(0, viewModel.NotifyPeriod.Value)
                Assert.Equal("Bar", viewModel.Theme.Value)
                Assert.Equal(2, count)

        viewModel.Load.Execute(null)             

// Tests fpr Feed ViewModel.
module FeedViewModelsTests =

    [<Fact>]
    let ``should create instance of feed viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<FeedViewModel>
        |> buildScope
        |> also Should.resolve<FeedViewModel>
        |> dispose

    [<Fact>]
    let ``should create instance of feed category viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<FeedCategoryViewModel>
        |> also registerAsSelf<SourceCategoryEntity>
        |> buildScope
        |> also Should.resolve<FeedCategoryViewModel>
        |> dispose

    [<Fact>]
    let ``should populate feed category with received articles``() =
        let fakeSources = [| SourceEntity(Uri="http://example.com") |] 
        let fakeEntity = SourceCategoryEntity(Title="Foo", Sources=fakeSources)

        let fakeFeedService =
            let response = 
                [ ArticleEntity(Title="Foo");
                  ArticleEntity(Title="Bar") ]
                |> fun s -> s.OrderBy(fun i -> i.Title) 
                |> Task.FromResult               
            let mockService = Mock<IFeedService>()
            mockService
                .Setup(fun i -> i.RetrieveFeedsAsync(It.IsAny<IEnumerable<SourceEntity>>()))
                .Returns(response) |> ignore
            mockService.Object     
        
        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also (registerInstanceAs<SourceCategoryEntity> fakeEntity)
            |> also (registerInstanceAs<IFeedService> fakeFeedService)
            |> also registerAsSelf<FeedCategoryViewModel>
            |> also registerAsSelf<FeedItemViewModel>
            |> buildScope

        let viewModel = resolve<FeedCategoryViewModel> scope
        viewModel.Fetch.CanExecuteChanged += fun _ ->
            if (viewModel.Fetch.CanExecute()) then
                
                Assert.Equal(2, viewModel.Items.Count)
                Assert.Equal("Bar", viewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", viewModel.Items.[1].Title.Value)
                Assert.Equal(false, viewModel.IsLoading.Value)
                Assert.Equal(false, viewModel.IsEmpty.Value)
                Assert.Equal("Foo", viewModel.Title.Value)

        viewModel.Fetch.Execute(null)  

    [<Fact>]
    let ``should populate feed viewmodel with items from db``() =
        let response = 
            let entities = [| SourceEntity(Uri="http://example.com") |]
            [ SourceCategoryEntity(Title="Foo", Sources=entities);
              SourceCategoryEntity(Title="Bar") ]
            |> fun s -> s.OrderBy(fun i -> i.Title)
            |> Task.FromResult
        
        let mockRepository = Mock<ISourcesRepository>()
        mockRepository
            .Setup(fun i -> i.GetAllAsync())
            .Returns(response) |> ignore

        use scope =
            ContainerBuilder()
            |> also registerDefaults  
            |> also (registerInstanceAs<ISourcesRepository> mockRepository.Object)
            |> also registerAsSelf<FeedCategoryViewModel>   
            |> also registerAsSelf<FeedViewModel>  
            |> buildScope

        let viewModel = resolve<FeedViewModel> scope 
        viewModel.Load.CanExecuteChanged += fun _ ->
            if (viewModel.Load.CanExecute()) then 
                
                Assert.Equal(2, viewModel.Items.Count)
                Assert.Equal("Bar", viewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", viewModel.Items.[1].Title.Value)

        viewModel.Load.Execute(null)                   

// Tests for favorite articles ViewModel
module FaveViewModelsTests =

    [<Fact>]
    let ``should create instance of fave viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<FaveViewModel>
        |> buildScope
        |> also Should.resolve<FaveViewModel> 
        |> dispose

    [<Fact>]
    let ``should create instance of fave item viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<ArticleEntity>
        |> also registerAsSelf<FeedItemViewModel>     
        |> buildScope
        |> also Should.resolve<FeedItemViewModel> 
        |> dispose

    [<Fact>]
    let ``should populate fave viewmodel with items from repo``() =
        let fakeRepository = 
            let fakeResults =
                [ ArticleEntity(Title="Foo", Fave=true);
                  ArticleEntity(Title="Bar", Fave=true) ]
                |> seq
                |> Task.FromResult
            let mockRepository = Mock<IArticlesRepository>()
            mockRepository
                .Setup(fun i -> i.GetAllAsync())
                .Returns(fakeResults)
                |> ignore
            mockRepository.Object

        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<FaveViewModel> 
            |> also registerAsSelf<FeedItemViewModel>
            |> also (registerInstanceAs<IArticlesRepository> fakeRepository)
            |> buildScope

        let viewModel = resolve<FaveViewModel> scope
        viewModel.Load.CanExecuteChanged += fun _ ->
            if (viewModel.Load.CanExecute()) then

                Assert.Equal(false, viewModel.IsLoading.Value)
                Assert.Equal(false, viewModel.IsEmpty.Value)
                Assert.Equal("Foo", viewModel.Items.[0].Title.Value)
                Assert.Equal("Bar", viewModel.Items.[1].Title.Value)

        viewModel.Load.Execute(null)

    [<Fact>]
    let ``should mark article as read and as unread``() = 
        let article = ArticleEntity(Read=false)
        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<FeedItemViewModel>
            |> also (registerInstanceAs<ArticleEntity> article)
            |> buildScope
        
        let viewModel = resolve<FeedItemViewModel> scope

        viewModel.MarkRead.Execute(null)
        Assert.Equal(true, article.Read)

        viewModel.MarkRead.Execute(null)
        Assert.Equal(false, article.Read)

    [<Fact>]
    let ``should mark article as favorite``() =
        let article = ArticleEntity(Fave=false)
        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<FeedItemViewModel>
            |> also (registerInstanceAs<ArticleEntity> article)
            |> buildScope

        let viewModel = resolve<FeedItemViewModel> scope
        viewModel.MarkFavorite.Execute(null)
        Assert.Equal(true, article.Fave)

// Tests for sources ViewModel.
module SourcesViewModelsTests =

    [<Fact>]
    let ``should resolve instance of sources viewmodel``() =
        ContainerBuilder()
        |> also registerDefaults
        |> also registerAsSelf<SourcesViewModel>
        |> buildScope
        |> also Should.resolve<SourcesViewModel>
        |> dispose

    [<Fact>]
    let ``child viewmodels should load properly``() =        
        let fakeSourcesRepository =
            let sourceCategoryEntities = 
                [ SourceCategoryEntity(Title="Foo");
                  SourceCategoryEntity(Title="Bar") ]
                |> fun s -> s.OrderBy(fun i -> i.Title)
                |> Task.FromResult
            let repositoryMock = Mock<ISourcesRepository>()
            repositoryMock
                .Setup(fun i -> i.GetAllAsync())
                .Returns(sourceCategoryEntities) 
                |> ignore
            repositoryMock.Object            
    
        use scope =
            ContainerBuilder()
            |> also registerDefaults
            |> also registerAsSelf<SourcesViewModel>
            |> also registerAsSelf<SourcesCategoryViewModel>
            |> also (registerInstanceAs<ISourcesRepository> fakeSourcesRepository)
            |> buildScope

        let viewModel = scope.Resolve<SourcesViewModel>()
        viewModel.Load.CanExecuteChanged += fun _ ->
            if viewModel.Load.CanExecute() then

                Assert.Equal(false, viewModel.IsLoading.Value)
                Assert.Equal(false, viewModel.IsEmpty.Value)
                Assert.Equal(2, viewModel.Items.Count)
                Assert.Equal("Bar", viewModel.Items.[0].Title.Value)
                Assert.Equal("Foo", viewModel.Items.[1].Title.Value)

        viewModel.Load.Execute(null)

    [<Fact>]
    let ``should resolve instance of sources category viewmodel``() =
        let category = SourceCategoryEntity(Title="Foo")
        use scope =
            ContainerBuilder()
            |> also registerDefaults
            |> also (registerInstanceAs<SourceCategoryEntity> category)
            |> also registerAsSelf<SourcesViewModel>
            |> also registerAsSelf<SourcesCategoryViewModel>
            |> buildScope

        let viewModel = resolve<SourcesCategoryViewModel> scope
        Assert.Equal("Foo", viewModel.Title.Value)

    [<Fact>]
    let ``should create instance of sources item viewmodel``() =
        let entity = SourceEntity(Notify=true, Uri="https://google.com")
        use scope = 
            ContainerBuilder()
            |> also registerDefaults    
            |> also registerAsSelf<SourcesCategoryViewModel>
            |> also registerAsSelf<SourcesItemViewModel>
            |> also registerAsSelf<SourcesViewModel>
            |> also registerAsSelf<SourceCategoryEntity>
            |> also (registerInstanceAs<SourceEntity> entity)
            |> buildScope

        let viewModel = resolve<SourcesItemViewModel> scope
        Assert.Equal("https://google.com", viewModel.Url.Value)
        Assert.Equal(true, viewModel.Notify.Value)

    [<Fact>]
    let ``should add entity to selected source category entity via viewmodel``() =
        let category = SourceCategoryEntity(Title="Foo")
        use scope = 
            ContainerBuilder()
            |> also registerDefaults
            |> also registerModule<ViewModelsModule>
            |> also registerAsSelf<SourcesViewModel>
            |> also registerAsSelf<SourcesCategoryViewModel>
            |> also (registerInstanceAs<SourceCategoryEntity> <| category)
            |> buildScope

        let sourcesRepository = resolve<ISourcesRepository> scope  
        sourcesRepository.InsertAsync category |> awaitTask

        let sourcesCategoryViewModel = resolve<SourcesCategoryViewModel> scope
        sourcesCategoryViewModel.SourceUri.Value <- "http://foo.bar"
        sourcesCategoryViewModel.AddSource.Execute()

        let all = await <| sourcesRepository.GetAllAsync()
        let first = all |> Seq.item 0

        Assert.Equal("Foo", first.Title)
        Assert.Equal(1, first.Sources |> Seq.length)
        Assert.Equal("http://foo.bar", first.Sources |> Seq.item 0 |> fun i -> i.Uri)

        sourcesRepository.RemoveAsync first |> awaitTask
        