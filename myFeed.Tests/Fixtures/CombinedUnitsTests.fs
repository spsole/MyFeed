module myFeed.Tests.Fixtures.CombinedUnitsTests

open Xunit
open DryIoc
open myFeed.Tests.Extensions
open myFeed.Services.Abstractions
open myFeed.Services
open myFeed.ViewModels
open myFeed.ViewModels.Implementations

[<Fact>]
let ``all default services should be registered``() = 

    use container = new Container()
    container.RegisterServices()
    container
    |> also registerMocks
    |> also Should.resolve<ICategoryStoreService>
    |> also Should.resolve<IFavoriteStoreService>
    |> also Should.resolve<ISettingStoreService>
    |> also Should.resolve<ISearchService>
    |> also Should.resolve<IOpmlService>
    |> also Should.resolve<ISerializationService>
    |> also Should.resolve<IImageService>
    |> also Should.resolve<IFeedFetchService>
    |> also Should.resolve<IFavoriteService>
    |> also Should.resolve<IFactoryService>
    |> also Should.resolve<IFeedStoreService>
    |> also Should.resolve<ISettingService>
    |> also Should.resolve<IDefaultsService>
    |> also Should.resolve<IBackgroundService>
    |> dispose

[<Fact>]
let ``all default viewmodels should be registered``() =

    use container = new Container()
    container.RegisterServices()
    container.RegisterViewModels()
    container    
    |> also registerMocks
    |> also Should.resolve<FaveViewModel>
    |> also Should.resolve<FeedViewModel>
    |> also Should.resolve<SearchViewModel>
    |> also Should.resolve<SettingsViewModel>
    |> also Should.resolve<ChannelsViewModel>
    |> dispose    
