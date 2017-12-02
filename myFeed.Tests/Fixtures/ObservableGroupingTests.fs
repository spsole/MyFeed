module myFeed.Tests.Fixtures.ObservableGroupingTests

open Xunit
open myFeed.ViewModels.Bindables
open myFeed.Tests.Extensions

[<Fact>]
let ``should create truly observable grouping``() =

    let grouping = ObservableGrouping<_, _>("Foo", [])
    grouping.CollectionChanged += fun _ -> Should.equal grouping.[0] 42 
    grouping.Add 42
