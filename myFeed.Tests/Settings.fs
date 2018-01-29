module myFeed.Tests.Fixtures.Setting

open Xunit
open NSubstitute
open myFeed.Models
open myFeed.Services
open myFeed.Interfaces
open myFeed.Tests.Extensions
open System.Collections.Generic
open System.Threading.Tasks
open System

[<Theory>]
[<InlineData(10.0)>]
[<InlineData(20.21)>]
[<InlineData(30.320)>]
[<CleanUpCollection("Settings")>]
let ``should save settings`` size =
    
    let settings = Settings(Font = size)
    let service = produce<LiteSettingManager> [connection]
    service.Write(settings).Wait()
    Should.equal size (service.Read().Result.Font)

[<Fact>]
[<CleanUpCollection("Settings")>]
let ``should resolve default values``() =

    let service = produce<LiteSettingManager> [connection]
    let settings = service.Read().Result
    Should.equal 100 settings.Max
    Should.equal 60 settings.Period
    Should.equal false settings.Banners
    Should.equal true settings.Images
    Should.equal 17. settings.Font
    
[<Theory>]
[<InlineData(10.0)>]
[<InlineData(20.21)>]
[<InlineData(30.320)>]
[<CleanUpCollection("Settings")>]
let ``should save settings to non-volatile storage`` size =

    let settings = Settings(Font = size)
    let service = produce<LiteSettingManager> [connection]
    service.Write(settings).Wait()
    
    let settings = connection.GetCollection<Settings>()
    let setting = settings.FindOne(fun x -> true)
    Should.equal size setting.Font
    
[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Bar", "Foo")>]
[<CleanUpCollection("Settings")>]
let ``should put only one entry in database collection`` key value =
    
    let settings = Settings(Font = 10., Max = 20)
    let service = produce<LiteSettingManager> [connection]
    for _ in [0..10] do service.Write(settings).Wait()
    connection.GetCollection<Settings>().FindAll()
    |> (List.ofSeq >> List.length)
    |> Should.equal 1
    