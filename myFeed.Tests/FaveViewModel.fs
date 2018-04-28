module myFeed.Tests.Fixtures.FaveViewModel

open Xunit
open NSubstitute
open myFeed.Models
open myFeed.ViewModels
open myFeed.Interfaces
open myFeed.Tests.Extensions
open System.Threading.Tasks
open System.Linq
open System

let private settings = Substitute.For<ISettingManager>()
settings.Read().Returns(Settings()) |> ignore

let private factory = 
    Func<IGrouping<string, Article>, FaveGroupViewModel>(fun x -> 
        produce<FaveGroupViewModel> [x;
            Func<Article, FeedItemViewModel>(fun x -> 
                produce<FeedItemViewModel> [x]) ])

[<Fact>]
let ``should load and group items``() =

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAll().Returns(Task.FromResult<seq<_>> [Article(Fave=true)]) |> ignore
    let faveViewModel = produce<FaveViewModel> [favorites; settings; factory]
    faveViewModel.Load.Invoke().Wait()
    
    Should.equal 1 faveViewModel.Items.Count
    Should.equal 1 faveViewModel.Items.[0].Items.Count

[<Fact>]
let ``should notify of loading property changed``() =

    let favorites = Substitute.For<IFavoriteManager>()
    favorites.GetAll().Returns(Task.FromResult(Seq.empty)) |> ignore
    
    let mutable changed = false
    let faveViewModel = produce<FaveViewModel> [favorites; settings; factory]
    faveViewModel.PropertyChanged += fun _ -> changed <- true
    faveViewModel.Load.Invoke().Wait()
    Should.equal changed true
    