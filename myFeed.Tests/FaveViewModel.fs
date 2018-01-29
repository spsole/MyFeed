module myFeed.Tests.Fixtures.FaveViewModel

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

let private factory = Substitute.For<IFactoryService>() 
factory.Create<Func<Article, ArticleViewModel>>().Returns(
    Func<Article, ArticleViewModel>(
        fun x -> produce<ArticleViewModel> [x]))
    |> ignore
factory.Create<Func<IGrouping<string, Article>, FaveGroupViewModel>>().Returns(
    Func<IGrouping<string, Article>, FaveGroupViewModel>(
        fun x -> produce<FaveGroupViewModel> [x]))
    |> ignore

[<Fact>]
let ``should notify of loading property changed``() =

    let factory = Substitute.For<IFactoryService>()
    factory.Create<Func<Article, ArticleViewModel>>().Returns(
        Func<Article, ArticleViewModel>(fun x -> produce<ArticleViewModel> [x]))
        |> ignore

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAllAsync().Returns([] |> Task.FromResult<seq<_>>) |> ignore
    
    let mutable changed = false
    let faveViewModel = produce<FaveViewModel> [factory; favorites]
    faveViewModel.PropertyChanged += fun _ -> changed <- true
    faveViewModel.Load.Invoke().Wait()
    Should.equal changed true
    
[<Fact>]
let ``should load and group items``() =

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAllAsync().Returns([ Article() ] 
        |> Task.FromResult<seq<_>>) |> ignore
    
    let faveViewModel = produce<FaveViewModel> [factory; favorites]
    faveViewModel.Load.Invoke().Wait()
    
    Should.equal 1 faveViewModel.Items.Count
    Should.equal 1 faveViewModel.Items.[0].Items.Count
