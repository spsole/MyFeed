module myFeed.Tests.BackgroundService

open Xunit
open NSubstitute
open myFeed.Models
open myFeed.Interfaces
open myFeed.Services
open myFeed.Platform
open myFeed.Tests
open System.Threading.Tasks
open System.Collections.Generic
open System 

[<Fact>]
let ``should send ordered notifications for articles with greater date``() =

    let store = Substitute.For<IFeedStoreService>()
    store.Load(Arg.Any()).Returns(Task.FromResult
        ([ Article(Title="Foo", PublishedDate=DateTime.Now);
          Article(Title="Bar", PublishedDate=DateTime.MaxValue) ] :> seq<_>)) |> ignore

    let settings = Substitute.For<ISettingManager>()
    settings.Read().Returns(Settings(Fetched = DateTime.MinValue)) |> ignore

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

    let store = Substitute.For<IFeedStoreService>()
    store.Load(Arg.Any()).Returns(Task.FromResult
        ([ Article(PublishedDate=DateTime.MinValue) ] :> seq<_>)) |> ignore
        
    let settings = Substitute.For<ISettingManager>()
    settings.Read().Returns(Settings()) |> ignore

    let mutable received = null
    let notify = Substitute.For<INotificationService>()
    notify.When(fun x -> x.SendNotifications(Arg.Any()) |> ignore)
          .Do(fun x -> received <- x.Arg<List<Article>>())

    let service = produce<BackgroundService> [store; notify; settings]
    service.CheckForUpdates(DateTime.Now).Wait()
    Should.equal 0 received.Count 
    