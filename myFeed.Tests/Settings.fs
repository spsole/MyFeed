module myFeed.Tests.Fixtures.Setting

open Xunit
open NSubstitute
open myFeed.Models
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
[<CleanUpCollection("Setting")>]
let ``should support all convertible values serialization`` key value =

    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<IConvertible>(key, value).Wait()
    Should.equal (string value) (service.GetAsync<string>(key).Result)
    
[<Theory>]
[<InlineData("Byte", 1uy)>]
[<InlineData("Char", 'x')>]
[<InlineData("Integer", 42)>]
[<InlineData("Float", 42.53)>]
[<InlineData("Boolean", true)>]
[<CleanUpCollection("Setting")>]
let ``should update database file when managing values`` key value =
    
    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<IConvertible>(key, value).Wait()
    let real = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
    Should.equal (value, key) (real.[0].Value, real.[0].Key)
    
[<Theory>]
[<InlineData("Foo", "StoredFoo")>]
[<InlineData("Bar", "StoredBar")>]
[<CleanUpCollection("Setting")>]
let ``should resolve values from defaults`` key value =    
    
    let defaults = Substitute.For<IDefaultsService>()
    defaults.DefaultSettings.Returns(dict [ key, value ] |> Dictionary<_, _>) |> ignore
    let service = produce<LiteSettingManager> [defaults; connection]
    Should.equal value (service.GetAsync<string>(key).Result)

[<Theory>]
[<InlineData("Foo", "StoredFoo")>]
[<InlineData("Bar", "StoredBar")>]
[<CleanUpCollection("Setting")>]
let ``should put new value into cache when set method is called`` key value =

    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<string>(key, value).Wait()
    Should.equal value (service.GetAsync<string>(key).Result) 
   
[<Theory>]
[<InlineData("Bar", 0)>]
[<InlineData("Foo", 10)>]
[<InlineData("Foo", 42)>]
[<CleanUpCollection("Setting")>]
let ``should support integer numbers serialization`` key value =

    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<int>(key, value).Wait()
    Should.equal value (service.GetAsync<int>(key).Result) 

[<Theory>]
[<InlineData("Foo", 42.53)>]
[<InlineData("Bar", 0.123)>]
[<CleanUpCollection("Setting")>]
let ``should support floating point numbers serialization`` key value =

    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<float>(key, value).Wait()
    Should.equal value (service.GetAsync<float>(key).Result)
    
[<Theory>]
[<InlineData("Foo", 1uy)>]
[<InlineData("Bar", 50uy)>]
[<CleanUpCollection("Setting")>]
let ``should support bytes serialization`` key value =

    let service = produce<LiteSettingManager> [connection]
    service.SetAsync<byte>(key, value).Wait()
    Should.equal value (service.GetAsync<byte>(key).Result) 
    
[<Theory>]
[<InlineData("UnsetKey")>]
[<InlineData("AnotherOne")>]
[<CleanUpCollection("Setting")>]
let ``should throw when trying to extract unset value`` key =    

    let service = produce<LiteSettingManager> [connection]
    fun () -> service.GetAsync<string>(key).Wait()
    |> Should.throw<AggregateException>
    
[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Bar", "Foo")>]
[<CleanUpCollection("Setting")>]
let ``should put only one entry in database collection`` key value =
    
    let service = produce<LiteSettingManager> [connection]
    for _ in [0..10] do service.SetAsync<string>(key, value).Wait()
    connection.GetCollection<Setting>().FindAll()
    |> (List.ofSeq >> List.length)
    |> Should.equal 1
   