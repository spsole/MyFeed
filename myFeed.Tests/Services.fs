namespace myFeed.Tests.Services

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Collections.Immutable
open System.IO
open System.Linq

open Xunit
open Autofac
open NSubstitute

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dep

open myFeed.ViewModels.Extensions

open myFeed.Services.Abstractions
open myFeed.Services.Implementations

open myFeed.Repositories.Abstractions

open myFeed.Entities.Opml
open myFeed.Entities.Local

// Tests for default settings service which is responsible
// for settings caching and setting defaults if not set.
type SettingsServiceFixture() =
    let settingsService = SettingsService(Substitute.For<_>(), Substitute.For<_>())

    [<Fact>]
    member x.``should be able to resolve strings``() = 
        settingsService.Set<string>("FooStr", "Bar").Wait()
        settingsService.Get<string>("FooStr").Result
        |> Should.equal "Bar" 

    [<Fact>]
    member x.``should be able to resolve booleans``() =
        settingsService.Set<bool>("FooBool", true).Wait()
        settingsService.Get<bool>("FooBool").Result
        |> Should.equal true

    [<Fact>]
    member x.``should be able to resolve ints``() =
        settingsService.Set<int>("FooInt", 42).Wait()
        settingsService.Get<int>("FooInt").Result
        |> Should.equal 42

    [<Fact>]
    member x.``should be able to resolve doubles``() =
        settingsService.Set<double>("Foo", 42.).Wait()
        settingsService.Get<double>("Foo").Result
        |> Should.equal 42.

    [<Fact>]
    member x.``should be able to resolve floats``() =
        settingsService.Set<float>("Foo", 42.).Wait()
        settingsService.Get<float>("Foo").Result
        |> Should.equal 42.

    [<Fact>]
    member x.``should be able to resolve bytes``() =
        settingsService.Set<byte>("Boo", 1uy).Wait()
        settingsService.Get<byte>("Boo").Result
        |> Should.equal 1uy

    [<Fact>]
    member x.``should resolve default settings``() =
        let defaultsService = Substitute.For<IDefaultsService>()
        defaultsService.DefaultSettings.Returns(dict["Foo", "Bar"] 
            |> Dictionary<string, string>) |> ignore
        let service = SettingsService(Substitute.For<_>(), defaultsService)  
        service.Get<string>("Foo").Result

    [<Fact>]
    member x.``should store recently fetched entities in cache``() =  
        let mutable counter = 0
        let configurationRepository = Substitute.For<IConfigurationRepository>()
        configurationRepository
            .GetByNameAsync(Arg.Any())
            .Returns(Task.FromResult "Foo")
            .AndDoes(fun _ -> counter <- counter + 1) |> ignore
        let service = SettingsService(configurationRepository, Substitute.For<_>())
        Should.equal "Foo" (service.Get<string>("Any").Result)
        Should.equal "Foo" (service.Get<string>("Any").Result)
        Should.equal 1 counter

// Tests for service responsible for retrieving feeds.
type FeedStoreServiceFixture() = 
    let createService (stored: seq<_>) (response: seq<_>) =
        let fetchService = Substitute.For<IFeedFetchService>()
        fetchService.FetchAsync(Arg.Any()).Returns(Task.FromResult struct(null, response)) |> ignore

        let articlesRepository = Substitute.For<IArticlesRepository>()
        articlesRepository.InsertAsync(Arg.Any()).Returns(Task.CompletedTask) |> ignore
        articlesRepository.GetAllAsync().Returns(Task.FromResult stored) |> ignore

        FeedStoreService(articlesRepository, fetchService)

    [<Fact>]
    member x.``should sort and filter article entities``() =
        let fakeSourceEntity = SourceEntity(Uri="http://foo.com")
        let fakeStoredArticles =
            [| ArticleEntity(Title="Foo", PublishedDate=DateTime.MinValue, Source=fakeSourceEntity);
               ArticleEntity(Title="Kek", PublishedDate=DateTime.Now, Source=fakeSourceEntity) |]
            |> also (Seq.iter fakeSourceEntity.Articles.Add)           

        let fakeFetchedEntities = 
            [ ArticleEntity(Title="Foo", PublishedDate=DateTime.MinValue);
              ArticleEntity(Title="Bar", PublishedDate=DateTime.MaxValue);
              ArticleEntity(Title="Abc", PublishedDate=DateTime.Now-TimeSpan.FromDays(1.)) ] 

        let service = createService fakeStoredArticles fakeFetchedEntities
        let feed = (List.ofSeq << snd) <| service.GetAsync([| fakeSourceEntity |]).Result

        Should.equal 4 (feed.Count())
        Should.equal "Bar" feed.[0].Title
        Should.equal "Kek" feed.[1].Title
        Should.equal "Abc" feed.[2].Title
        Should.equal "Foo" feed.[3].Title

    [<Fact>]
    member x.``should filter articles not related to any source``() =
        let fakeSourceEntity = SourceEntity(Uri="http://foo.bar")
        let fakeStoredArticles =
            [ ArticleEntity(Title="Foo", PublishedDate=DateTime.Now, Source=fakeSourceEntity);
              ArticleEntity(Title="Bar", PublishedDate=DateTime.Now) ]
        fakeSourceEntity.Articles.Add fakeStoredArticles.[0]                 

        let service = createService fakeStoredArticles []
        let feed = (List.ofSeq << snd) <| service.GetAsync([| fakeSourceEntity |]).Result

        Should.equal "Foo" feed.[0].Title
        Should.equal 1 (feed.Count())

    [<Fact>]
    member x.``should fetch new articles and remove not related to any source``() =
        let fakeSourceEntity = SourceEntity(Uri="http://foo.bar")
        let fakeStoredArticles =
            [ ArticleEntity(Title="Foo", Source=fakeSourceEntity);
              ArticleEntity(Title="Bar", PublishedDate=DateTime.Now) ]
        fakeSourceEntity.Articles.Add fakeStoredArticles.[0]            

        let fakeFetchedArticles = 
            [ ArticleEntity(Title="FooBar", PublishedDate=DateTime.Now);
              ArticleEntity(Title="Jumba") ]

        let service = createService fakeStoredArticles fakeFetchedArticles
        let feed = (List.ofSeq << snd) <| service.GetAsync([| fakeSourceEntity |]).Result

        Should.equal 3 (feed.Count())
        Should.equal "FooBar" feed.[0].Title
        Should.equal "Jumba" feed.[2].Title
        Should.equal "Foo" feed.[1].Title

// Tests for opml service.
type OpmlServiceFixture() =

    [<Fact>]
    member x.``should be able to export opml feeds``() =
        let fakeResponse = 
            [ SourceCategoryEntity(Title="Foo"); SourceCategoryEntity(Title="Bar", 
                Sources=[| SourceEntity(Uri="http://example.com/rss") |]) ]
            |> fun sequence -> sequence.OrderBy(fun i -> i.Title)
            |> Task.FromResult

        let sourcesRepository = Substitute.For<ISourcesRepository>()
        sourcesRepository.GetAllAsync().Returns(fakeResponse) |> ignore
        let filePickerService = Substitute.For<IFilePickerService>()
        filePickerService.PickFileForWriteAsync().Returns(new MemoryStream() :> Stream |> Task.FromResult) |> ignore
        let serializationService = Substitute.For<ISerializationService>()

        let service = 
            OpmlService(
                Substitute.For<_>(),
                sourcesRepository,
                filePickerService,
                Substitute.For<_>(),
                serializationService)
        service.ExportOpmlFeeds().Wait()

        let opml = serializationService.ReceivedCalls().First().GetArguments().[0] :?> Opml
        Should.equal 2 opml.Body.Count
        Should.equal "Bar" opml.Body.[0].Title
        Should.equal "Foo" opml.Body.[1].Title
        Should.equal 1 opml.Body.[0].ChildOutlines.Count
        Should.equal "example.com" opml.Body.[0].ChildOutlines.[0].Title
        Should.equal "http://example.com" opml.Body.[0].ChildOutlines.[0].HtmlUrl
        Should.equal "http://example.com/rss" opml.Body.[0].ChildOutlines.[0].XmlUrl

    [<Fact>]
    member x.``should be able to import opml feeds``() =    
        let outlines = 
            [ Outline(XmlUrl="http://foo.com");
              Outline(XmlUrl="https://bar.com") ] |> List<_>

        let serializationService = Substitute.For<ISerializationService>()
        serializationService.Deserialize<Opml>(Arg.Any()).Returns(Opml(Body=outlines)) |> ignore
        let sourcesRepository = Substitute.For<ISourcesRepository>()
        sourcesRepository.InsertAsync(Arg.Any()).Returns(Task.CompletedTask) |> ignore

        let service = 
            OpmlService(
                Substitute.For<_>(),
                sourcesRepository,
                Substitute.For<_>(),
                Substitute.For<_>(),
                serializationService)
        service.ImportOpmlFeeds().Wait()

        let entities = sourcesRepository.ReceivedCalls().First().GetArguments().[0] :?> SourceCategoryEntity[]
        Should.equal 2 entities.[0].Sources.Count 
        Should.equal "http://foo.com" (entities.[0].Sources |> Seq.item 0 |> fun i -> i.Uri)
        Should.equal "https://bar.com" (entities.[0].Sources |> Seq.item 1 |> fun i -> i.Uri)

// Tests for html parsing service responsible for images extracting.
type RegexExtractImageServiceFixture(service: RegexExtractImageService) =
    interface IClassFixture<RegexExtractImageService>

    [<Fact>]
    member x.``should extract first image url from plain text``() =
        "Foo <bla a='42'></bla> \n<img foo='bar' src='http://example.com' />"
        |> (service.ExtractImage >> Should.equal "http://example.com")        

    [<Fact>]
    member x.``should return null if there are no images``() =
        "London is the capital of Great Britain"
        |> (service.ExtractImage >> Should.equal null)

// Xml file stream serializer test fixture. 
type XmlSerializerFixture(service: SerializationService) =
    interface IClassFixture<SerializationService>

    [<Fact>]
    member x.``should serialize typed objects into xml``() =
        let filename = "sample.xml"
        let instance = Opml(Head=Head(Title="Foo"))
        service.Serialize<Opml>(instance, File.OpenWrite filename)
        Should.contain "Foo" (File.ReadAllText filename)
        File.Delete filename

    [<Fact>]
    member x.``should deserialize typed objects from xml``() =
        let filename = "sample.xml"
        let instance = Opml(Head=Head(Title="Bar"))
        service.Serialize<Opml>(instance, File.OpenWrite filename)
        let opml = service.Deserialize<Opml>(File.OpenRead filename)
        Should.equal "Bar" opml.Head.Title
        File.Delete filename

// Tests for feed search engine based on Feedly API.
type FeedlySearchServiceFixture(service: FeedlySearchService) =
    interface IClassFixture<FeedlySearchService>

    [<Fact>]
    member x.``should find something on feedly search engine``() =
        service.Search("feedly").Result |> Should.notEqual null

// Tests for observable properties!
type ObservablePropertyFixture() =

    [<Fact>] 
    member x.``property changed event should raise on value change``() =
        let property = Property(42)
        let mutable fired = 0
        property.PropertyChanged += fun e -> fired <- fired + 1
        property.Value <- 3
        Should.equal 1 fired 

    [<Fact>]
    member x.``property changed event should not fire if value is the same``() =
        let property = Property(42)
        let mutable fired = 0
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 42
        Should.equal 0 fired

    [<Fact>]
    member x.``property name should be value``() =
        let property = Property(42)
        property.PropertyChanged += fun e -> Should.equal "Value" e.PropertyName
        property.Value <- 3    
        
    [<Fact>]
    member x.``should slowly initialize value via fun of task``() =
        let property = Property<string>(fun () -> "Foo" |> Task.FromResult)
        Should.equal "Foo" property.Value
        
// Tests for task-based ICommand implementation.
type AwaitableCommandTests() =

    [<Fact>]
    member x.``should execute passed actions``() =
        let mutable fired = 0
        let command = Func<Task>(fun () -> fired <- fired + 1; Task.CompletedTask) |> Command
        command.Execute(null)
        Should.equal true (command.CanExecute()) 
        Should.equal 1 fired

    [<Fact>]
    member x.``should await previous execution``() =
        let command = Func<Task>(fun () -> Task.Delay(1000)) |> Command
        command.Execute(null)
        Should.equal false (command.CanExecute())

    [<Fact>]
    member x.``should raise state change event``() =
        let mutable fired = 0
        let command = Func<Task>(fun () -> Task.CompletedTask) |> Command
        command.CanExecuteChanged += fun _ -> fired <- fired + 1
        command.Execute(null)
        Should.equal 2 fired   