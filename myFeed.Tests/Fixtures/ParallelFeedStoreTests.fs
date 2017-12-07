module myFeed.Tests.Fixtures.ParallelFeedStoreTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions
open System.Threading.Tasks
open System

[<Fact>]
let ``should sort stored article entities``() = 

    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any<_>()).Returns(
        Seq.empty<Article> 
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
        |> List.ofSeq              

    Should.equal 3 articles.Length
    Should.equal "Zoo" articles.[0].Title
    Should.equal "Foo" articles.[1].Title
    Should.equal "Bar" articles.[2].Title

[<Fact>]
let ``should save fetched article entities``() =    

    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any<_>()).Returns(
        [Article(Title="Foo")] :> seq<_>
        |> Task.FromResult)
        |> ignore

    let mutable articlesInserted = null
    let categories = Substitute.For<ICategoryStoreService>()
    categories.When(fun x -> x.InsertArticleRangeAsync(Arg.Any<_>(), Arg.Any<_>()) |> ignore)
              .Do(fun x -> articlesInserted <- x.Arg<seq<Article>>())       

    let service = produce<ParallelFeedStoreService> [fetcher; categories; settings]
    let articles = 
        service.LoadAsync(
            [ Channel(Uri="http://foo.bar") ]).Result
            |> List.ofSeq            

    let article = Seq.item 0 articlesInserted
    Should.equal 1 articles.Length
    Should.equal "Foo" articles.[0].Title
    Should.equal "Foo" article.Title

[<Fact>]
let ``should mix and order fetched and stored articles by date``() = 
    
    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(5) |> ignore

    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any<_>()).Returns(
        [Article(Title="Foo", PublishedDate=DateTime.Now)] :> seq<_>
        |> Task.FromResult)
        |> ignore

    let service = produce<ParallelFeedStoreService> [fetcher; settings]
    let articles =
        service.LoadAsync(
            [ Channel(Articles=toList
                [ Article(Title="Bar", PublishedDate=
                    DateTime.MinValue)])]).Result
        |> List.ofSeq

    Should.equal 2 articles.Length
    Should.equal "Foo" articles.[0].Title
    Should.equal "Bar" articles.[1].Title         

[<Fact>]
let ``should remove outdated articles if count is greater than custom``() = 

    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore

    let articles = Seq.init 200 (fun _ -> Article())
    let channel = Channel(Articles=toList articles)
    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any()).Returns(Seq.empty 
        |> Task.FromResult) |> ignore
    
    let service = produce<ParallelFeedStoreService> [fetcher; settings]
    let articles = service.LoadAsync([channel]).Result |> List.ofSeq

    Should.equal 70 articles.Length

[<Fact>]
let ``should remove articles with minimum publishing date only``() =    

    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore

    let articles = Seq.init 200 (fun _ -> Article())
    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any()).Returns(articles 
        |> Task.FromResult) |> ignore
    
    let service = produce<ParallelFeedStoreService> [fetcher; settings]
    let articles = service.LoadAsync([Channel()]).Result |> List.ofSeq

    Should.equal 70 articles.Length
    
[<Fact>]
let ``should ignore whitespaces while comparing titles``() =
    
    let settings = Substitute.For<ISettingService>()
    settings.GetAsync<_>(Arg.Any()).Returns(70) |> ignore
    
    let fetcher = Substitute.For<IFeedFetchService>()
    fetcher.FetchAsync(Arg.Any<_>()).Returns(
        [ Article(Title="Foo  ", FeedTitle="Bar");
          Article(Title="Bar\r\n", FeedTitle="Foo")] :> seq<_>
        |> Task.FromResult) |> ignore
    
    let service = produce<ParallelFeedStoreService> [fetcher; settings]
    let articles = 
        service.LoadAsync(
            [ Channel(Articles=toList 
                [ Article(Title="Foo", FeedTitle="Bar");
                  Article(Title="Bar", FeedTitle="Foo") ]) ]).Result 
                |> List.ofSeq
    
    Should.equal 2 articles.Length
    Should.equal "Foo" articles.[0].Title
    Should.equal "Bar" articles.[1].Title
    Should.equal "Bar" articles.[0].FeedTitle
    Should.equal "Foo" articles.[1].FeedTitle
