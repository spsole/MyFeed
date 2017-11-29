module myFeed.Tests.Services.CacheableSettingTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Tests.Extensions
open System.Collections.Generic
open System.Threading.Tasks
open System

[<Fact>]
let ``should resolve default values from received defaults service``() =    
    
    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict
        [ "Foo", "StoredFoo"; 
          "Bar", "StoredBar" ] |> Dictionary<_, _>) 
        |> ignore

    let service = produce<CacheableSettingService> [defaults]
    service.GetAsync("Foo").Result |> Should.equal "StoredFoo"
    service.GetAsync("Bar").Result |> Should.equal "StoredBar"

[<Fact>]
let ``should put new value into cache when set method is called``() =

    let service = produce<CacheableSettingService> []
    service.SetAsync("Zoo", "StoredZoo").Wait()
    service.GetAsync("Zoo").Result |> Should.equal "StoredZoo"
    
[<Fact>]
let ``should support generic convertible numbers serialization``() =

    let service = produce<CacheableSettingService> []
    service.SetAsync("Foo", 42).Wait()
    service.GetAsync<int>("Foo").Result |> Should.equal 42    

[<Fact>]
let ``should support floating point numbers serialization``() =

    let service = produce<CacheableSettingService> []
    service.SetAsync<float>("Foo", 42.53).Wait()
    service.GetAsync<float>("Foo").Result |> Should.equal 42.53
    
[<Fact>]
let ``should support bytes serialization also``() =

    let service = produce<CacheableSettingService> []
    service.SetAsync<byte>("Foo", 1uy).Wait()
    service.GetAsync<byte>("Foo").Result |> Should.equal 1uy

[<Fact>]
let ``should throw when trying to extract unknown value``() =    

    let service = produce<CacheableSettingService> []
    fun () -> service.GetAsync("Unknown").Result |> ignore
    |> Should.throw<AggregateException>
    
[<Fact>]
let ``should query the database only once when using get method``() =

    let mutable counter = 0
    let settings = Substitute.For<ISettingStoreService>() 
    settings.When(fun x -> x.GetByKeyAsync("Foo") |> ignore)
            .Do(fun _ -> counter <- counter + 1)

    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict["Foo", "Bar"] |> Dictionary<_, _>) |> ignore

    let service = produce<CacheableSettingService> [settings; defaults]
    service.GetAsync("Foo").Result |> Should.equal "Bar"
    service.GetAsync("Foo").Result |> Should.equal "Bar"
    service.GetAsync("Foo").Result |> Should.equal "Bar"
    counter |> Should.equal 1

[<Fact>]
let ``should lock repository access from multiple threads and query db only once``() =    

    let mutable counter = 0
    let settings = Substitute.For<ISettingStoreService>() 
    settings.When(fun x -> x.GetByKeyAsync("Foo") |> ignore)
            .Do(fun _ -> counter <- counter + 1)

    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict["Foo", "Bar"] |> Dictionary<_, _>) |> ignore

    let service = produce<CacheableSettingService> [settings; defaults]

    // Access service getter 3 times at once from different threads.
    let thread = fun () -> Task.Run(fun () -> service.GetAsync("Foo").Result)
    Task.WhenAll([thread(); thread(); thread()]) |> ignore

    // Counter should equal 1, not four. Lock should work properly.
    service.GetAsync("Foo").Result |> Should.equal "Bar"
    Should.equal 1 counter