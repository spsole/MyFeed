namespace myFeed.Tests.ViewModels

open Xunit

open System
open System.Threading.Tasks

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Domain
open myFeed.Tests.Extensions.Dependency 

open myFeed.Repositories.Models
open myFeed.Repositories.Abstractions

open myFeed.ViewModels.Bindables
open myFeed.ViewModels.Implementations

module ObservablePropertyFixture =

    [<Fact>] 
    let ``should raise property change event on value change``() =

        let mutable fired = 0
        let property = ObservableProperty(42)
        property.PropertyChanged += fun e -> fired <- fired + 1
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
