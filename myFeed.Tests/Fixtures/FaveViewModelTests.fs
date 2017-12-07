module myFeed.Tests.Fixtures.FaveViewModelTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.ViewModels.Implementations
open myFeed.Services.Implementations
open myFeed.Tests.Extensions
open myFeed.Services.Models
open System.Threading.Tasks
open System

let private mockFactory = Substitute.For<IFactoryService>()  
mockFactory.CreateInstance<ArticleViewModel>(
    Arg.Any()).Returns(fun x -> 
        x.Arg<obj[]>() 
        |> DryIocStateContainer 
        |> Seq.singleton<obj> 
        |> produce<ArticleViewModel>) 
        |> ignore

[<Theory>]
[<InlineData("Foo")>]
[<InlineData("Bar")>]
let ``should populate view model with items received from repository`` title =   

    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns([ Article(Title=title) ] 
        :> seq<_> |> Task.FromResult) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; mockFactory]
    faveViewModel.Load.Invoke().Wait()

    Should.equal 1 faveViewModel.Items.Count
    Should.equal title faveViewModel.Items.[0].[0].Title.Value 

[<Theory>]
[<InlineData("A", "B", "C")>]
[<InlineData("Bar", "Foo", "Zoo")>]
let ``should order items in view model by name descending`` first second third =    
    
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(FeedTitle=first); 
          Article(FeedTitle=third); 
          Article(FeedTitle=second); ] 
        :> seq<_> |> Task.FromResult) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; mockFactory]
    faveViewModel.Load.Invoke().Wait()
    faveViewModel.OrderByFeed.Invoke().Wait()

    Should.equal 3 faveViewModel.Items.Count
    Should.equal third faveViewModel.Items.[0].[0].Feed.Value
    Should.equal second faveViewModel.Items.[1].[0].Feed.Value
    Should.equal first faveViewModel.Items.[2].[0].Feed.Value
  
[<Fact>]
let ``should order items in view model by date descending``() =    
    
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(Title="A", PublishedDate=DateTime.MaxValue); 
          Article(Title="C", PublishedDate=DateTime.MinValue); 
          Article(Title="B", PublishedDate=DateTime.Now); ] 
        :> seq<_> |> Task.FromResult) |> ignore
        
    let faveViewModel = produce<FaveViewModel> [favorites; mockFactory]
    faveViewModel.Load.Invoke().Wait()
    faveViewModel.OrderByDate.Invoke().Wait()

    Should.equal 3 faveViewModel.Items.Count
    Should.equal "A" faveViewModel.Items.[0].[0].Title.Value
    Should.equal "B" faveViewModel.Items.[1].[0].Title.Value
    Should.equal "C" faveViewModel.Items.[2].[0].Title.Value

[<Fact>]
let ``should be able to delete and restore items from groupings``() =    

    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(Fave=true, PublishedDate=DateTime.MinValue); 
          Article(Fave=true, PublishedDate=DateTime.Now);] 
        :> seq<_> |> Task.FromResult) |> ignore
        
    let faveViewModel = produce<FaveViewModel> [favorites; mockFactory]
    faveViewModel.Load.Invoke().Wait()

    let item = faveViewModel.Items.[0].[0]
    item.IsFavorite.Value <- false
    Should.equal 1 faveViewModel.Items.Count
    Should.equal 1 faveViewModel.Items.[0].Count
    item.IsFavorite.Value <- true
    Should.equal 2 faveViewModel.Items.Count
