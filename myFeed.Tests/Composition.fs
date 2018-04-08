module myFeed.Tests.Fixtures.Composition

open Xunit
open DryIoc
open myFeed
open myFeed.Tests.Extensions
open myFeed.Interfaces
open myFeed.Services
open myFeed.ViewModels

[<Fact>]
let ``all default services and view models should be registered``() = 

    use container = new Container()
    container.RegisterShared()
    container
    |> also registerMocks
    |> also Should.resolve<ICategoryManager>
    |> also Should.resolve<ISearchService>
    |> also Should.resolve<IOpmlService>
    |> also Should.resolve<ISerializationService>
    |> also Should.resolve<IImageService>
    |> also Should.resolve<IFeedFetchService>
    |> also Should.resolve<IFavoriteManager>
    |> also Should.resolve<IFeedStoreService>
    |> also Should.resolve<ISettingManager>
    |> also Should.resolve<IBackgroundService>
    |> also Should.resolve<FaveViewModel>
    |> also Should.resolve<FeedViewModel>
    |> also Should.resolve<SearchViewModel>
    |> also Should.resolve<SettingViewModel>
    |> also Should.resolve<ChannelViewModel>
    |> dispose
