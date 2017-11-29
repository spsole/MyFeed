namespace myFeed.Tests.ViewModels

open Xunit    
open NSubstitute
open System
open System.Threading.Tasks
open myFeed.ViewModels.Bindables
open myFeed.ViewModels.Implementations
open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Domain
open myFeed.Tests.Extensions.Dependency
open myFeed.Services.Abstractions
open myFeed.Services.Models

module ObservablePropertyFixture =

    [<Fact>] 
    let ``should raise property change event on value change``() =

        let mutable fired = 0
        let property = ObservableProperty(42)
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 3
        Should.equal 1 fired 

    [<Fact>]
    let ``should not fire property canged event if value is the same``() =
    
        let mutable fired = 0
        let property = ObservableProperty(42)
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 42
        Should.equal 0 fired

    [<Fact>]
    let ``should treat property name as value string``() =

        let property = ObservableProperty(42)
        property.PropertyChanged += fun e -> Should.equal "Value" e.PropertyName
        property.Value <- 3    
        
    [<Fact>]
    let ``should slowly initialize value via funtion returning task``() =

        let property = ObservableProperty<string>(fun () -> "Foo" |> Task.FromResult)
        Should.equal "Foo" property.Value    

module ObservableCommandFixture =

    [<Fact>]
    let ``should execute passed actions``() =

        let mutable fired = 0
        let command = Func<Task>(fun () -> 
            fired <- fired + 1 
            Task.CompletedTask) |> ObservableCommand
        command.Execute()
        Should.equal true (command.CanExecute()) 
        Should.equal 1 fired

    [<Fact>]
    let ``should await previous execution``() =

        let command = Func<Task>(fun () -> Task.Delay(1000)) |> ObservableCommand
        command.Execute()
        Should.equal false (command.CanExecute())

    [<Fact>]
    let ``should raise state change event``() =

        let mutable fired = 0
        let command = Func<Task>(fun () -> Task.CompletedTask) |> ObservableCommand
        command.CanExecuteChanged += fun _ -> fired <- fired + 1
        command.Execute()
        Should.equal 2 fired 

module ObservableGroupingTests =

    [<Fact>]
    let ``should create truly observable grouping``() =

        let grouping = ObservableGrouping<_, _>("Foo", [])
        grouping.CollectionChanged += fun _ -> Should.equal grouping.[0] 42 
        grouping.Add 42

module FaveViewModelFixture =

    [<Fact>]
    let ``should populate view model with items received from repository``() =   

        let favorites = Substitute.For<IFavoritesRepository>()
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
        
        let favorites = Substitute.For<IFavoritesRepository>()
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
        
        let favorites = Substitute.For<IFavoritesRepository>()
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

        let favorites = Substitute.For<IFavoritesRepository>()
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
