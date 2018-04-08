module myFeed.Tests.Fixtures.FaveViewModel

open DryIoc
open Xunit
open NSubstitute
open ReactiveUI
open myFeed.Models
open myFeed.ViewModels
open myFeed.Interfaces
open myFeed.Services
open myFeed.Tests.Extensions
open System.Threading.Tasks
open System.Linq
open System

let private settings = Substitute.For<ISettingManager>()
settings.Read().Returns(Settings()) |> ignore

let private factory = Substitute.For<IResolver>() 
factory.Resolve<Func<Article, FeedItemViewModel>>().Returns(
    Func<Article, FeedItemViewModel>(fun x -> 
        produce<FeedItemViewModel> [x])) |> ignore

factory.Resolve<Func<IGrouping<string, Article>, FaveGroupViewModel>>().Returns(
    Func<IGrouping<string, Article>, FaveGroupViewModel>(fun x ->
        produce<FaveGroupViewModel> [x; factory])) |> ignore

[<Fact>]
let ``should load and group items``() =

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAllAsync().Returns(Task.FromResult<seq<_>> [Article(Fave=true)]) |> ignore
    let faveViewModel = produce<FaveViewModel> [factory; favorites; settings]
    faveViewModel.Load.Invoke().Wait()
    
    Should.equal 1 faveViewModel.Items.Count
    Should.equal 1 faveViewModel.Items.[0].Items.Count

[<Fact>]
let ``should notify of loading property changed``() =

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAllAsync().Returns(Task.FromResult(Seq.empty)) |> ignore
    
    let mutable changed = false
    let faveViewModel = produce<FaveViewModel> [factory; favorites; settings]
    faveViewModel.PropertyChanged += fun _ -> changed <- true
    faveViewModel.Load.Invoke().Wait()
    Should.equal changed true
    