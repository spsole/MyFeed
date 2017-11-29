module myFeed.Tests.Stores.SettingStoreTests

open Xunit
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions

let private repository = LiteDbSettingStoreService <| connection

[<Fact>]
[<CleanUpCollection("Setting")>]
let ``should insert settings into database``() =    

    repository.InsertAsync(Setting(Key="Foo", Value="Bar")).Wait()
    let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
    
    Should.equal "Bar" response.[0].Value
    Should.equal "Foo" response.[0].Key

[<Fact>]
[<CleanUpCollection("Setting")>]
let ``should return settings by their keys``() =

    repository.InsertAsync(Setting(Key="Foo", Value="Bar")).Wait()
    let setting = repository.GetByKeyAsync("Foo").Result   
    Should.equal "Bar" setting.Value   

[<Fact>]
[<CleanUpCollection("Setting")>]   
let ``should update settings in database``() =

    let setting = Setting(Key="Bar", Value="Foo")        
    repository.InsertAsync(setting).Wait()

    setting.Key <- "Foo"
    setting.Value <- "Bar"
    repository.UpdateAsync(setting).Wait()

    let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
    Should.equal "Foo" response.[0].Key
    Should.equal "Bar" response.[0].Value
