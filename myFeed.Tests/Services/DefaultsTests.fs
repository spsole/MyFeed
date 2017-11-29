module myFeed.Tests.Services.DefaultsTests

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services.Implementations

[<Fact>]
let ``should resolve all needed default settings``() =

    let service = produce<DefaultsService> []
    let settings = service.DefaultSettings
    
    settings.["LoadImages"] |> Should.notBeNull
    settings.["NeedBanners"] |> Should.notBeNull
    settings.["NotifyPeriod"] |> Should.notBeNull
    settings.["MaxArticlesPerFeed"] |> Should.notBeNull
    settings.["LastFetched"] |> Should.notBeNull
    settings.["FontSize"] |> Should.notBeNull
    settings.["Theme"] |> Should.notBeNull
