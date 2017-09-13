namespace myFeed.Tests.Services

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.IO
open System.Linq

open Xunit
open Autofac

open Moq
open Moq.Protected

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.DependencyInjection
open myFeed.Tests.Extensions.Mocking

open myFeed.ViewModels.Extensions

open myFeed.Services.Abstractions
open myFeed.Services.Implementations

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Entities.Opml
open myFeed.Repositories.Entities.Local

// Tests for service responsible for retrieving feeds.
module FeedServiceTests = 

    // Mocks feed service using provided repository instance and 
    let mockService (stored: seq<ArticleEntity>) (resp: SourceEntity * seq<ArticleEntity>) =
        let storedTask = stored |> Task.FromResult
        let respTask = resp.ToValueTuple() |> Task.FromResult

        // Mock repository.
        let mockRepository = Mock<IArticlesRepository>()
        mockRepository
            .Setup(fun i -> i.GetAllAsync())
            .Returns(storedTask) |> ignore  

        // Mock feed service virtual methods.        
        let mockFeedService = 
            Mock<FeedService>( 
                MockBehavior.Loose, // Params order matters!
                Mock<IHtmlParsingService>().Object :> obj,
                mockRepository.Object :> obj)
        mockFeedService.CallBase <- true
        mockFeedService.Protected()
            .Setup<Task<struct (SourceEntity * IEnumerable<ArticleEntity>)>>(
                "RetrieveFeedAsync", ItExpr.IsAny<SourceEntity>())
            .Returns(respTask) |> ignore          
        mockFeedService.Object      

    [<Fact>]
    let ``should sort and filter article entities``() =
        let fakeFetchedEntities = 
            [ ArticleEntity(Title="Foo", PublishedDate=DateTime.MinValue);
              ArticleEntity(Title="Bar", PublishedDate=DateTime.MaxValue);
              ArticleEntity(Title="Abc", PublishedDate=DateTime.Now); ] 

        // Create entities linked using both foreign keys: ICollection and reference.
        let fakeSourceEntity = SourceEntity(Uri="http://foo.com")
        let fakeStoredEntities =
            [ ArticleEntity(Title="Foo", PublishedDate=DateTime.MinValue, Source=fakeSourceEntity);
              ArticleEntity(Title="Kek", PublishedDate=DateTime.Now, Source=fakeSourceEntity)]
        fakeStoredEntities |> Seq.iter fakeSourceEntity.Articles.Add          

        let service = mockService fakeStoredEntities (fakeSourceEntity, fakeFetchedEntities)
        let feed = service.RetrieveFeedsAsync([ fakeSourceEntity ]) |@> List.ofSeq

        Assert.Equal(4, feed.Count())
        Assert.Equal("Bar", feed.[0].Title)
        Assert.Equal("Kek", feed.[1].Title)
        Assert.Equal("Abc", feed.[2].Title)
        Assert.Equal("Foo", feed.[3].Title)

    [<Fact>]
    let ``should filter articles not related to any source``() =
    
        // Create entities linked using both foreign keys: ICollection and reference.
        let fakeSource = SourceEntity(Uri="http://foo.bar")
        let fakeStoredArticles =
            [ ArticleEntity(Title="Foo", PublishedDate = DateTime.Now, Source=fakeSource);
              ArticleEntity(Title="Bar", PublishedDate = DateTime.Now) ]
        fakeSource.Articles.Add fakeStoredArticles.[0]    

        let service = mockService fakeStoredArticles (fakeSource, Seq.empty)     
        let feed = service.RetrieveFeedsAsync([ fakeSource ]) |@> List.ofSeq

        Assert.Equal(1, feed.Count())
        Assert.Equal("Foo", feed.[0].Title)

    [<Fact>]
    let ``should fetch new articles and remove not related to any source``() =    
        let fakeFetchedArticles = 
            [ ArticleEntity(Title="FooBar", PublishedDate = DateTime.Now);
              ArticleEntity(Title="Jumba") ]

        // Create entities linked using both foreign keys: ICollection and reference.
        let fakeSource = SourceEntity(Uri="http://foo.bar")
        let fakeStoredArticles =
            [ ArticleEntity(Title="Foo", Source = fakeSource);
              ArticleEntity(Title="Bar", PublishedDate = DateTime.Now) ]
        fakeSource.Articles.Add fakeStoredArticles.[0]    

        let service = mockService fakeStoredArticles (fakeSource, fakeFetchedArticles)
        let feed = service.RetrieveFeedsAsync([ fakeSource ]) |@> List.ofSeq

        Assert.Equal(3, feed.Count())
        Assert.Equal("FooBar", feed.[0].Title)
        Assert.Equal("Foo", feed.[1].Title)
        Assert.Equal("Jumba", feed.[2].Title)

    [<Fact>]
    let ``favorite articles should never be removed but not included into feed``() =
        let fakeStoredArticles =
            [ ArticleEntity(Title="Foo", Fave=false);
              ArticleEntity(Title="Bar", Fave=true) ]        
        
        let mockRepository = Mock<IArticlesRepository>()
        mockRepository
            .Setup(fun i -> i.GetAllAsync())
            .Returns(fakeStoredArticles :> seq<_> |> Task.FromResult) 
            |> ignore 
        mockRepository
            .Setup(fun i -> i.RemoveAsync(It.IsAny<ArticleEntity[]>()))
            .Returns(Task.CompletedTask)
            .Callback<seq<ArticleEntity>>(fun i -> 
               Assert.Equal(1, Seq.length i)) // Make sure that only one row is deleted.
            |> ignore        

        let mockFeedService = 
            Mock<FeedService>( 
                MockBehavior.Loose, // Params order matters!
                Mock<IHtmlParsingService>().Object :> obj,
                mockRepository.Object :> obj)

        let service = mockFeedService.Object
        let feed = service.RetrieveFeedsAsync(Seq.empty) |@> List.ofSeq
        Assert.Equal(0, feed.Count())

// Tests for html parsing service.
module HtmlParsingServiceTests =

    [<Fact>]
    let ``should extract first image url from plain text``() =
        let sample = 
            """ Foo bar <bla a="42"></bla> hello test
                <img foo="bar" src="http://example.com" /> """
        let service = HtmlParsingService()
        let extractedUrl = service.ExtractImageUrl(sample)
        Assert.Equal("http://example.com", extractedUrl)

// Tests for opml service.
module OpmlServiceTests =

    let registerOpmlDefaults (builder: ContainerBuilder) =
        builder 
        |> tee registerMockInstance<IPlatformService>
        |> tee registerMockInstance<ITranslationsService>
        |> tee registerMockInstance<ISourcesRepository>
        |> tee registerMockInstance<ISerializationService>
        |> ignore

    [<Fact>]
    let ``should create instance of opml service``() =
        ContainerBuilder()
        |> tee registerOpmlDefaults
        |> tee registerAs<OpmlService, IOpmlService>
        |> buildScope
        |> tee assertResolve<IOpmlService>
        |> dispose

    [<Fact>]
    let ``should be able to export opml feeds``() =
        let fakeResponse = 
            let fakeSourceEntities = 
                [ SourceEntity(Uri="http://example.com/rss") ]
                |> collection
            [ SourceCategoryEntity(Title="Foo", Sources=([] |> collection));
              SourceCategoryEntity(Title="Bar", Sources=fakeSourceEntities) ]
            |> fun ls -> ls.OrderBy(fun i -> i.Title)
            |> Task.FromResult

        let mockRepository = Mock<ISourcesRepository>()
        mockRepository
            .Setup(fun i -> i.GetAllAsync())
            .Returns(fakeResponse) |> ignore

        let mockPicker = Mock<IPlatformService>()
        mockPicker
            .Setup(fun i -> i.PickFileForWriteAsync())
            .Returns(new MemoryStream() :> Stream |> Task.FromResult) |> ignore                 

        let mockSerializer = Mock<ISerializationService>()
        mockSerializer
            .Setup(fun i -> i.Serialize<Opml>(It.IsAny<Opml>(), It.IsAny<Stream>()))
            .Callback<Opml, Stream>(fun o s -> 
                Assert.NotNull(o)
                Assert.Equal(2, o.Body.Count)
                Assert.Equal("Bar", o.Body.[0].Title)
                Assert.Equal("Foo", o.Body.[1].Title)
                Assert.Equal(1, o.Body.[0].ChildOutlines.Count)
                Assert.Equal("example.com", o.Body.[0].ChildOutlines.[0].Title)
                Assert.Equal("http://example.com", o.Body.[0].ChildOutlines.[0].HtmlUrl)
                Assert.Equal("http://example.com/rss", o.Body.[0].ChildOutlines.[0].XmlUrl)
            ) |> ignore

        use scope =
            ContainerBuilder()
            |> tee registerOpmlDefaults
            |> tee registerAsSelf<OpmlService>
            |> tee (registerInstanceAs<ISourcesRepository> mockRepository.Object)
            |> tee (registerInstanceAs<ISerializationService> mockSerializer.Object)
            |> tee (registerInstanceAs<IPlatformService> mockPicker.Object)
            |> buildScope

        let service = resolve<OpmlService> scope
        awaitTask <| service.ExportOpmlFeeds()

    [<Fact>]
    let ``should be able to import opml feeds``() =    
        let outlines = 
            [ Outline(XmlUrl="http://foo.com");
              Outline(XmlUrl="https://bar.com") ]
            |> collection              

        let mockSerializer = Mock<ISerializationService>()
        mockSerializer
            .Setup(fun i -> i.Deserialize<Opml>(It.IsAny<Stream>()))
            .Returns(Opml(Body=outlines))
            |> ignore

        let mockRepository = Mock<ISourcesRepository>()
        mockRepository
            .Setup(fun i -> i.InsertAsync(It.IsAny<SourceCategoryEntity[]>()))
            .Returns(Task.CompletedTask)
            .Callback<SourceCategoryEntity[]>(fun e -> 
                let entity = e |> Seq.item 0
                match entity.Sources.Count with 
                | 1 -> Assert.Equal("http://foo.com", entity.Sources |> Seq.item 0 |> fun i -> i.Uri)
                | 2 -> Assert.Equal("https://bar.com", entity.Sources |> Seq.item 1 |> fun i -> i.Uri)
                | _ -> failwith "Index out of range!"
            ) |> ignore
        
        use scope = 
            ContainerBuilder()
            |> tee registerOpmlDefaults
            |> tee registerAs<OpmlService, IOpmlService>
            |> tee (registerInstanceAs<ISerializationService> mockSerializer.Object)
            |> tee (registerInstanceAs<ISourcesRepository> mockRepository.Object)
            |> buildScope

        let service = resolve<IOpmlService> scope
        awaitTask <| service.ImportOpmlFeeds()      

// Tests for xml serializer.
module XmlSerializerTests =

    [<Fact>]
    let ``should resolve instance of xml serializer``() =
        ContainerBuilder()
        |> tee registerAs<SerializationService, ISerializationService>
        |> buildScope
        |> tee assertResolve<ISerializationService>
        |> dispose

    [<Fact>]
    let ``should serialize typed objects into xml``() =
        let serializer = SerializationService()
        let filename = "sample.xml"

        let instance = Opml()
        instance.Head <- Head()
        instance.Head.Title <- "Foo"

        serializer.Serialize<Opml>(instance, File.OpenWrite filename)
        Assert.Contains("Foo", File.ReadAllText filename)
        File.Delete filename

    [<Fact>]
    let ``should deserialize typed objects from xml``() =
        let serializer = SerializationService()
        let filename = "sample.xml"

        let instance = Opml()
        instance.Head <- Head()
        instance.Head.Title <- "Bar"

        serializer.Serialize<Opml>(instance, File.OpenWrite filename)
        let opml = serializer.Deserialize<Opml>(File.OpenRead filename)
        Assert.Equal("Bar", opml.Head.Title)
        File.Delete filename

// Tests for feed search engine based on Feedly API.
module FeedlySearchServiceTests =

    [<Fact>]
    let ``should find something on feedly``() =
        FeedlySearchService().Search("feedly")
        |@> Assert.NotNull

// Tests for observable properties!
module ObservablePropertyTests =

    [<Fact>] 
    let ``property changed event should raise on value change``() =
        let property = ObservableProperty(42)
        let mutable fired = 0
        property.PropertyChanged += fun e -> fired <- fired + 1
        property.Value <- 3
        Assert.Equal(1, fired)  

    [<Fact>]
    let ``property changed event should not fire if value is the same``() =
        let property = ObservableProperty(42)
        let mutable fired = 0
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 42
        Assert.Equal(0, fired)  

    [<Fact>]
    let ``property name should be value``() =
        let property = ObservableProperty(42)
        property.PropertyChanged += fun e -> Assert.Equal("Value", e.PropertyName)
        property.Value <- 3             
        
// Tests for task-based ICommand implementation.
module ActionCommandTests =

    [<Fact>]
    let ``action command should execute passed actions``() =
        let mutable fired = 0
        let command =
            Func<Task>(fun () -> 
                fired <- fired + 1
                Task.CompletedTask)
            |> ActionCommand
        Assert.Equal(true, command.CanExecute())    
        command.Execute(null)
        Assert.Equal(true, command.CanExecute()) 
        Assert.Equal(1, fired)

    [<Fact>]
    let ``action command should await previous execution``() =
        let command =
            Func<Task>(fun () -> Task.Delay(1000))
            |> ActionCommand
        Assert.Equal(true, command.CanExecute())
        command.Execute(null)
        Assert.Equal(false, command.CanExecute())

    [<Fact>]
    let ``action command should raise state change event``() =
        let mutable fired = 0
        let command =
            Func<Task>(fun () -> Task.CompletedTask)
            |> ActionCommand
        Assert.Equal(true, command.CanExecute())
        command.CanExecuteChanged += fun _ -> fired <- fired + 1
        command.Execute(null)
        Assert.Equal(2, fired)      