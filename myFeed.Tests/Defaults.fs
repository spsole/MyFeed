module myFeed.Tests.Fixtures.Defaults

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services

[<Theory>]
[<InlineData("LoadImages")>]
[<InlineData("NeedBanners")>]
[<InlineData("NotifyPeriod")>]
[<InlineData("MaxArticlesPerFeed")>]
[<InlineData("LastFetched")>]
[<InlineData("FontSize")>]
[<InlineData("Theme")>]
let ``should resolve all needed default settings`` setting =

    let service = produce<DefaultsService> []
    let settings = service.DefaultSettings
    Should.notBeNull settings.[setting]
    