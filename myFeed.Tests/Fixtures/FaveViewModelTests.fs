module myFeed.Tests.Fixtures.FaveViewModelTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.ViewModels.Implementations
open myFeed.Tests.Extensions
open myFeed.Services.Models
open System.Threading.Tasks
open System

[<Fact>]
let ``should populate view model with items received from repository``() =   

    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns([ Article(Title="Foo") ] 
        :> seq<_> |> Task.FromResult) |> ignore

    let factory = Substitute.For<IFactoryService>()    
    factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
        [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; factory]
    faveViewModel.Load.CanExecuteChanged += fun _ -> 
        if faveViewModel.Load.CanExecute() then
            Should.equal 1 faveViewModel.Items.Count
            Should.equal "Foo" faveViewModel.Items.[0].[0].Title.Value 
    faveViewModel.Load.Execute()
    
[<Fact>]
let ``should order items in view model by name descending``() =    
    
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(FeedTitle="C"); Article(FeedTitle="A"); Article(FeedTitle="B"); ] 
        :> seq<_> |> Task.FromResult) |> ignore
        
    let factory = Substitute.For<IFactoryService>()    
    factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
        [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; factory]
    faveViewModel.Load.CanExecuteChanged += fun _ ->
        if faveViewModel.Load.CanExecute() then
            faveViewModel.OrderByFeed.CanExecuteChanged += fun _ -> 
                if faveViewModel.OrderByFeed.CanExecute() then
                    Should.equal 3 faveViewModel.Items.Count
                    Should.equal "A" faveViewModel.Items.[0].[0].Feed.Value
                    Should.equal "B" faveViewModel.Items.[1].[0].Feed.Value
                    Should.equal "C" faveViewModel.Items.[2].[0].Feed.Value
            faveViewModel.OrderByFeed.Execute() 
    faveViewModel.Load.Execute() 

[<Fact>]
let ``should order items in view model by date descending``() =    
    
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(Title="A", PublishedDate=DateTime.MaxValue); 
          Article(Title="C", PublishedDate=DateTime.MinValue); 
          Article(Title="B", PublishedDate=DateTime.Now); ] 
        :> seq<_> |> Task.FromResult) |> ignore
        
    let factory = Substitute.For<IFactoryService>()    
    factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
        [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; factory]
    faveViewModel.Load.CanExecuteChanged += fun _ ->
        if faveViewModel.Load.CanExecute() then
            faveViewModel.OrderByDate.CanExecuteChanged += fun _ -> 
                if faveViewModel.OrderByDate.CanExecute() then
                    Should.equal 3 faveViewModel.Items.Count
                    Should.equal "A" faveViewModel.Items.[0].[0].Title.Value
                    Should.equal "B" faveViewModel.Items.[1].[0].Title.Value
                    Should.equal "C" faveViewModel.Items.[2].[0].Title.Value
            faveViewModel.OrderByDate.Execute() 
    faveViewModel.Load.Execute()       

[<Fact>]
let ``should be able to delete and restore items from groupings``() =    

    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.GetAllAsync().Returns(
        [ Article(Fave=true, PublishedDate=DateTime.MinValue); 
          Article(Fave=true, PublishedDate=DateTime.Now);] 
        :> seq<_> |> Task.FromResult) |> ignore
        
    let factory = Substitute.For<IFactoryService>()    
    factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
        [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

    let faveViewModel = produce<FaveViewModel> [favorites; factory]
    faveViewModel.Load.CanExecuteChanged += fun _ ->
        if faveViewModel.Load.CanExecute() then
            let item = faveViewModel.Items.[0].[0]
            item.IsFavorite.Value <- false
            Should.equal 1 faveViewModel.Items.Count
            Should.equal 1 faveViewModel.Items.[0].Count
            item.IsFavorite.Value <- true
            Should.equal 2 faveViewModel.Items.Count
    faveViewModel.Load.Execute()
