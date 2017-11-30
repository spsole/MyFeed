module myFeed.Tests.Services.CacheableSettingTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Tests.Extensions
open System.Collections.Generic
open System.Threading.Tasks
open System

[<Theory>]
[<InlineData("Byte", 1uy)>]
[<InlineData("Char", 'x')>]
[<InlineData("Integer", 42)>]
[<InlineData("Float", 42.53)>]
[<InlineData("Boolean", true)>]
let ``should support all convertible values serialization`` key value =

    let service = produce<CacheableSettingService> []
    service.SetAsync<IConvertible>(key, value).Wait()
    Should.equal (string value) (service.GetAsync<string>(key).Result)
    
[<Theory>]
[<InlineData("Foo", "StoredFoo")>]
[<InlineData("Bar", "StoredBar")>]
let ``should resolve values from defaults`` key value =    
    
    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict [ key, value ] |> Dictionary<_, _>) |> ignore
    let service = produce<CacheableSettingService> [defaults]
    Should.equal value (service.GetAsync<string>(key).Result)

[<Theory>]
[<InlineData("Foo", "StoredFoo")>]
[<InlineData("Bar", "StoredBar")>]
let ``should put new value into cache when set method is called`` key value =

    let service = produce<CacheableSettingService> []
    service.SetAsync<string>(key, value).Wait()
    Should.equal value (service.GetAsync<string>(key).Result) 
   
[<Theory>]
[<InlineData("Bar", 0)>]
[<InlineData("Foo", 10)>]
[<InlineData("Foo", 42)>]
let ``should support integer numbers serialization`` key value =

    let service = produce<CacheableSettingService> []
    service.SetAsync<int>(key, value).Wait()
    Should.equal value (service.GetAsync<int>(key).Result) 

[<Theory>]
[<InlineData("Foo", 42.53)>]
[<InlineData("Bar", 0.123)>]
let ``should support floating point numbers serialization`` key value =

    let service = produce<CacheableSettingService> []
    service.SetAsync<float>(key, value).Wait()
    Should.equal value (service.GetAsync<float>(key).Result)
    
[<Theory>]
[<InlineData("Foo", 1uy)>]
[<InlineData("Bar", 50uy)>]
let ``should support bytes serialization`` key value =

    let service = produce<CacheableSettingService> []
    service.SetAsync<byte>(key, value).Wait()
    Should.equal value (service.GetAsync<byte>(key).Result) 
    
[<Theory>]
[<InlineData("UnsetKey")>]
[<InlineData("AnotherOne")>]
let ``should throw when trying to extract unset value`` key =    

    let service = produce<CacheableSettingService> []
    fun () -> service.GetAsync<string>(key).Wait()
    |> Should.throw<AggregateException>
    
[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Bar", "Foo")>]
let ``should query the database only once`` key value =

    let mutable counter = 0
    let settings = Substitute.For<ISettingStoreService>() 
    settings.When(fun x -> x.GetByKeyAsync(key) |> ignore)
            .Do(fun _ -> counter <- counter + 1)

    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict[key, value] 
        |> Dictionary<_, _>) |> ignore

    let service = produce<CacheableSettingService> [settings; defaults]
    service.GetAsync(key).Result |> Should.equal value
    service.GetAsync(key).Result |> Should.equal value
    service.GetAsync(key).Result |> Should.equal value
    Should.equal 1 counter

[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Key", "Value")>]
let ``should handle concurrent queries correctly`` key value =   
    
    let mutable counter = 0
    let settings = Substitute.For<ISettingStoreService>() 
    settings.When(fun x -> x.GetByKeyAsync(key) |> ignore)
            .Do(fun _ -> counter <- counter + 1)

    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict[key, value] 
        |> Dictionary<_, _>) |> ignore

    let service = produce<CacheableSettingService> [settings; defaults]
    let thread = fun () -> Task.Run(fun () -> service.GetAsync<string>(key).Wait())
    Task.WhenAll([thread(); thread(); thread()]).Wait() |> ignore
    Should.equal 1 counter
