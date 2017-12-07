module myFeed.Tests.Fixtures.SettingStoreTests

open Xunit
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions

let private repository = LiteSettingStoreService connection

[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Boo", "Hi, world!")>]
[<CleanUpCollection("Setting")>]
let ``should insert settings into database`` key value =    

    repository.InsertAsync(Setting(Key=key, Value=value)).Wait()
    let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
    Should.equal value response.[0].Value
    Should.equal key response.[0].Key

[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Boo", "Hi, world!")>]
[<CleanUpCollection("Setting")>]
let ``should return settings by their keys`` key value =

    repository.InsertAsync(Setting(Key=key, Value=value)).Wait()
    let setting = repository.GetByKeyAsync(key).Result   
    Should.equal value setting.Value   

[<Theory>]
[<InlineData("Foo", "Bar")>]
[<InlineData("Boo", "Hi, world!")>]
[<CleanUpCollection("Setting")>]   
let ``should update settings in database`` key value =

    let setting = Setting(Key=value, Value=key)        
    repository.InsertAsync(setting).Wait()

    setting.Key <- key
    setting.Value <- value
    repository.UpdateAsync(setting).Wait()

    let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
    Should.equal value response.[0].Value
    Should.equal key response.[0].Key
