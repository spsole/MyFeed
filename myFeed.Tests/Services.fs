namespace myFeed.Tests.Services

open Xunit
open Autofac
open NSubstitute
open System
open System.IO
open System.Linq
open System.Threading.Tasks
open System.Collections.Generic
open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Domain
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Services.Platform
open myFeed.Services.Models

module FeedlySearchServiceFixture =

    [<Fact>]
    let ``should always return valid response``() =

        let service = produce<FeedlySearchService> []
        service.SearchAsync("Foo").Result 
        |> Should.notEqual null

module FeedReaderFetchServiceFixture =

    [<Fact>]
    let ``should always return valid response``() =

        let service = produce<FeedReaderFetchService> []
        service.FetchAsync("nonsense").Result 
        |> also (fst >> Should.notEqual null)   
        |> also (snd >> Should.notEqual null) 
        |> ignore

module RegexImageServiceFixture =

    [<Fact>]
    let ``should extract first image url from plain text``() =

        let service = produce<RegexImageService> []
        "Foo <bla a='42'></bla> \n<img foo='bar' src='http://example.com' />"
        |> (service.ExtractImageUri >> Should.equal "http://example.com")            

    [<Fact>]
    let ``should return null if there are no images``() =
    
        let service = produce<RegexImageService> []
        "London is the capital of Great Britain"
        |> (service.ExtractImageUri >> Should.equal null)

    [<Fact>]
    let ``should return exactly first match from text``() =    

        let service = produce<RegexImageService> []
        "?<img src='http://bar.foo' /> <img src='http://foo.bar' />"
        |> (service.ExtractImageUri >> Should.equal "http://bar.foo")

    [<Fact>]
    let ``should return null if passed string is null``() =

        let service = produce<RegexImageService> []
        null |> (service.ExtractImageUri >> Should.equal null)    
        
module XmlSerializationServiceFixture =

    [<Fact; CleanUpFile("sample")>]
    let ``should serialize typed objects into xml``() =

        let service = produce<XmlSerializationService> []
        let instance = Opml(Head=OpmlHead(Title="Foo"))
        service.Serialize<Opml>(instance, File.OpenWrite "sample")
        Should.contain "Foo" (File.ReadAllText "sample")

    [<Fact; CleanUpFile("sample")>]
    let ``should deserialize typed objects from xml``() =

        let service = produce<XmlSerializationService> []
        let instance = Opml(Head=OpmlHead(Title="Bar"))
        service.Serialize<Opml>(instance, File.OpenWrite "sample")
        let opml = service.Deserialize<Opml>(File.OpenRead "sample")
        Should.equal "Bar" opml.Head.Title

module ParallelFeedStoreServiceFixture =

    [<Fact>]
    let ``should sort stored article entities``() = 

        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any<_>()).Returns(
            (null, Seq.empty<Article>) 
            |> Task.FromResult)
            |> ignore

        let service = produce<ParallelFeedStoreService> [fetcher; settings]
        let articles =
            service.LoadAsync(
                [ Channel(Articles=toList
                    [ Article(Title="Foo", PublishedDate=DateTime.Now);
                      Article(Title="Bar", PublishedDate=DateTime.MinValue);
                      Article(Title="Zoo", PublishedDate=DateTime.MaxValue) ]
                  )]).Result
            |> (snd >> List.ofSeq)              

        Should.equal 3 articles.Length
        Should.equal "Zoo" articles.[0].Title
        Should.equal "Foo" articles.[1].Title
        Should.equal "Bar" articles.[2].Title

    [<Fact>]
    let ``should save fetched article entities``() =    

        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any<_>()).Returns(
            (null, [Article(Title="Foo")] :> seq<_>) 
            |> Task.FromResult)
            |> ignore

        let mutable articlesInserted = null
        let categories = Substitute.For<ICategoriesRepository>()
        categories.When(fun x -> x.InsertArticleRangeAsync(Arg.Any<_>(), Arg.Any<_>()) |> ignore)
                  .Do(fun x -> articlesInserted <- x.Arg<seq<Article>>())       

        let service = produce<ParallelFeedStoreService> [fetcher; categories; settings]
        let articles = 
            service.LoadAsync(
                [ Channel(Uri="http://foo.bar") ]).Result
                |> (snd >> List.ofSeq)            

        let article = Seq.item 0 articlesInserted
        Should.equal 1 articles.Length
        Should.equal "Foo" articles.[0].Title
        Should.equal "Foo" article.Title

    [<Fact>]
    let ``should mix and order fetched and stored articles by date``() = 
        
        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any<_>()).Returns(
            (null, [Article(Title="Foo", PublishedDate=DateTime.Now)] :> seq<_>) 
            |> Task.FromResult)
            |> ignore

        let service = produce<ParallelFeedStoreService> [fetcher; settings]
        let articles =
            service.LoadAsync(
                [ Channel(Articles=toList
                    [ Article(Title="Bar", PublishedDate=
                        DateTime.MinValue)])]).Result
            |> (snd >> List.ofSeq)

        Should.equal 2 articles.Length
        Should.equal "Foo" articles.[0].Title
        Should.equal "Bar" articles.[1].Title         

    [<Fact>]
    let ``should remove outdated articles if count is greater than custom``() = 

        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore

        let articles = Seq.init 200 (fun _ -> Article())
        let channel = Channel(Articles=toList articles)
        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any()).Returns((null, Seq.empty) 
            |> Task.FromResult) |> ignore
        
        let service = produce<ParallelFeedStoreService> [fetcher; settings]
        let articles = service.LoadAsync([channel]).Result |> (snd >> List.ofSeq)

        Should.equal 70 articles.Length

    [<Fact>]
    let ``should remove articles with minimum publishing date only``() =    
    
        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore

        let articles = Seq.init 200 (fun _ -> Article())
        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any()).Returns((null, articles) 
            |> Task.FromResult) |> ignore
        
        let service = produce<ParallelFeedStoreService> [fetcher; settings]
        let articles = service.LoadAsync([Channel()]).Result |> (snd >> List.ofSeq)

        Should.equal 70 articles.Length
        
    [<Fact>]
    let ``should ignore whitespaces while comparing titles``() =
        
        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore
        
        let fetcher = Substitute.For<IFeedFetchService>()
        fetcher.FetchAsync(Arg.Any<_>()).Returns(
            (null, [ Article(Title="Foo  ", FeedTitle="Bar");
                     Article(Title="Bar\r\n", FeedTitle="Foo")] :> seq<_>) 
            |> Task.FromResult) |> ignore
        
        let service = produce<ParallelFeedStoreService> [fetcher; settings]
        let articles = 
            service.LoadAsync(
                [ Channel(Articles=toList 
                    [ Article(Title="Foo", FeedTitle="Bar");
                      Article(Title="Bar", FeedTitle="Foo") ]) ]).Result 
                    |> (snd >> List.ofSeq)
        
        Should.equal 2 articles.Length
        Should.equal "Foo" articles.[0].Title
        Should.equal "Bar" articles.[1].Title
        Should.equal "Bar" articles.[0].FeedTitle
        Should.equal "Foo" articles.[1].FeedTitle

module FavoritesServiceFixture =

    [<Fact>]
    let ``should update article entity fave value when adding and removing``() =
        
        let article = Article(Fave=false)        
        let service = produce<FavoritesService> []
        
        service.Insert(article).Wait()
        Should.equal article.Fave true

        service.Remove(article).Wait()
        Should.equal article.Fave false

    [<Fact>]
    let ``should insert and remove article via repository``() =
    
        let mutable deleted = 0
        let mutable inserted = 0

        let favorites = Substitute.For<IFavoritesRepository>()
        favorites.When(fun x -> x.InsertAsync(Arg.Any<_>()) |> ignore)
                 .Do(fun _ -> inserted <- inserted + 1)
        favorites.When(fun x -> x.RemoveAsync(Arg.Any<_>()) |> ignore)
                 .Do(fun _ -> deleted <- deleted + 1)             

        let article = Article(Fave=false)
        let service = produce<FavoritesService> [favorites]

        service.Insert(article).Wait()
        service.Insert(article).Wait()
        service.Remove(article).Wait()
        service.Remove(article).Wait()

        Should.equal 1 deleted
        Should.equal 1 inserted

module AutofacFactoryServiceFixture =

    type Sample(name: string) = member __.Name = name

    [<Fact>]
    let ``should inject parameters with given type``() =

        let containerBuilder = ContainerBuilder()
        containerBuilder.RegisterType<Sample>().AsSelf() |> ignore
        let lifetimeScope = containerBuilder.Build()

        let factory = produce<AutofacFactoryService> [lifetimeScope]
        let instance = factory.CreateInstance<Sample> "Foo"
        Should.equal "Foo" instance.Name

module DefaultsServiceFixture =

    [<Fact>]
    let ``should resolve all needed default settings``() =

        let service = produce<DefaultsService> []
        let settings = service.DefaultSettings
        
        settings.["LoadImages"] |> Should.notBeNull
        settings.["NeedBanners"] |> Should.notBeNull
        settings.["NotifyPeriod"] |> Should.notBeNull
        settings.["MaxArticlesPerFeed"] |> Should.notBeNull
        settings.["LastFetched"] |> Should.notBeNull
        settings.["FontSize"] |> Should.notBeNull
        settings.["Theme"] |> Should.notBeNull

module BackgroundServiceFixture = 

    [<Fact>]
    let ``should send ordered notifications for articles with greater date``() =

        let articles = [ Article(Title="Foo", PublishedDate=DateTime.Now);
                         Article(Title="Bar", PublishedDate=DateTime.MaxValue) ]
        let store = Substitute.For<IFeedStoreService>()
        store.LoadAsync(Arg.Any()).Returns( 
            (null, articles :> seq<_>) 
            |> Task.FromResult) 
            |> ignore

        let settings = Substitute.For<ISettingsService>()
        settings.GetAsync(Arg.Any()).Returns(
            DateTime.MinValue |> Task.FromResult)
            |> ignore      

        let mutable received = null
        let notify = Substitute.For<INotificationService>()
        notify.When(fun x -> x.SendNotifications(Arg.Any()) |> ignore)
              .Do(fun x -> received <- x.Arg<List<Article>>())

        let service = produce<BackgroundService> [store; settings; notify]
        service.CheckForUpdates(DateTime.Now).Wait()

        Should.equal "Foo" received.[0].Title
        Should.equal "Bar" received.[1].Title

    [<Fact>]
    let ``should not send notifications for outdated old articles``() =

        let articles = [ Article(Title="Foo", PublishedDate=DateTime.MinValue) ]
        let store = Substitute.For<IFeedStoreService>()
        store.LoadAsync(Arg.Any()).Returns(
            (null, articles :> seq<_>) 
            |> Task.FromResult) 
            |> ignore

        let mutable received = null
        let notify = Substitute.For<INotificationService>()
        notify.When(fun x -> x.SendNotifications(Arg.Any()) |> ignore)
              .Do(fun x -> received <- x.Arg<List<Article>>())

        let service = produce<BackgroundService> [store; notify]
        service.CheckForUpdates(DateTime.Now).Wait()

        Should.equal 0 received.Count 
