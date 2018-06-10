module myFeed.Tests.LiteSettingManager

open Xunit
open myFeed.Models
open myFeed.Services
open myFeed.Tests

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
    let setting = settings.FindOne(fun _ -> true)
    Should.equal size setting.Font
    
[<Fact>]
[<CleanUpCollection("Settings")>]
let ``should put only one entry in database collection``() =
    
    let settings = Settings(Font = 10., Max = 20)
    let service = produce<LiteSettingManager> [connection]
    for _ in [0..10] do service.Write(settings).Wait()
    connection.GetCollection<Settings>().FindAll()
    |> (List.ofSeq >> List.length)
    |> Should.equal 1

[<Fact>]
[<CleanUpCollection("Settings")>]
let ``should resolve default values``() =

    let service = produce<LiteSettingManager> [connection]
    let settings = service.Read().Result
    Should.notBeNull settings.Max
    Should.notBeNull settings.Period
    Should.notBeNull settings.Banners
    Should.notBeNull settings.Images
    Should.notBeNull settings.Font
    