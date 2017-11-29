module myFeed.Tests.ViewModels.ObservablePropertyTests

open Xunit
open myFeed.ViewModels.Bindables
open myFeed.Tests.Extensions
open System.Threading.Tasks

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