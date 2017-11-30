module myFeed.Tests.Services.BackgroundTests

open Xunit
open NSubstitute
open myFeed.Services.Models
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Services.Platform
open myFeed.Tests.Extensions
open System.Threading.Tasks
open System.Collections.Generic
open System 

[<Fact>]
let ``should send ordered notifications for articles with greater date``() =

    let articles = [ Article(Title="Foo", PublishedDate=DateTime.Now);
                     Article(Title="Bar", PublishedDate=DateTime.MaxValue) ]
          
    let store = Substitute.For<IFeedStoreService>()
    store.LoadAsync(Arg.Any()).Returns((null, articles :> seq<_>) 
        |> Task.FromResult) |> ignore

    let settings = Substitute.For<ISettingService>()
    settings.GetAsync(Arg.Any()).Returns(DateTime.MinValue 
        |> Task.FromResult) |> ignore      

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

    let articles = Seq.singleton <| Article(PublishedDate=DateTime.MinValue)
    let store = Substitute.For<IFeedStoreService>()
    store.LoadAsync(Arg.Any()).Returns(Task.FromResult(null, articles)) |> ignore

    let mutable received = null
    let notify = Substitute.For<INotificationService>()
    notify.When(fun x -> x.SendNotifications(Arg.Any()) |> ignore)
          .Do(fun x -> received <- x.Arg<List<Article>>())

    let service = produce<BackgroundService> [store; notify]
    service.CheckForUpdates(DateTime.Now).Wait()
    Should.equal 0 received.Count 
