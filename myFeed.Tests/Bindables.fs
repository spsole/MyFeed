module myFeed.Tests.Fixtures.Bindables

open Xunit
open myFeed.Tests.Extensions
open System.Threading.Tasks
open myFeed.Common

[<Fact>]
let ``should execute passed actions``() =

    let mutable fired = 0
    let command = ObservableCommand(fun () -> fired <- fired + 1; Task.CompletedTask)
    command.Execute()
    Should.equal true (command.CanExecute()) 
    Should.equal 1 fired

[<Fact>]
let ``should await until command execution is finished``() =

    let command = ObservableCommand(fun () -> Task.Delay(100))
    command.Execute()
    Should.equal false (command.CanExecute())

[<Fact>]
let ``should raise state change event``() =

    let mutable fired = 0
    let command = ObservableCommand(fun () -> Task.CompletedTask)
    command.CanExecuteChanged += fun _ -> fired <- fired + 1
    command.Execute()
    Should.equal 2 fired 
    
[<Fact>]
let ``should create truly observable grouping``() =

    let grouping = ObservableGrouping<_, _>("Foo", [])
    grouping.CollectionChanged += fun _ -> Should.equal grouping.[0] 42 
    grouping.Add 42

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
let ``should treat property name as 'value' string``() =

    let property = ObservableProperty(42)
    property.PropertyChanged += fun e -> Should.equal "Value" e.PropertyName
    property.Value <- 3    
    
[<Fact>]
let ``should slowly initialize value via funtion returning task``() =

    let property = ObservableProperty<string>(fun () -> "Foo" |> Task.FromResult)
    Should.equal "Foo" property.Value  
    