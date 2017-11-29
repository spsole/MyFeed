namespace myFeed.Tests.Modules
    
open Xunit
open Autofac
open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dependency
open myFeed.Tests.Extensions.Domain
open myFeed.Services
open myFeed.Services.Abstractions
open myFeed.ViewModels.Implementations
open myFeed.ViewModels

module CombinedUnitsFixture =   

    [<Fact>]
    let ``all default services should be registered``() = 

        ContainerBuilder()
        |> also registerModule<ServicesModule> 
        |> also registerMocks
        |> build
        |> also Should.resolve<ISearchService>
        |> also Should.resolve<IOpmlService>
        |> also Should.resolve<ISerializationService>
        |> also Should.resolve<IImageService>
        |> also Should.resolve<IFeedFetchService>
        |> also Should.resolve<IFavoritesService>
        |> also Should.resolve<IFactoryService>
        |> also Should.resolve<IFeedStoreService>
        |> also Should.resolve<ISettingsService>
        |> also Should.resolve<IDefaultsService>
        |> also Should.resolve<IBackgroundService>
        |> dispose

    [<Fact>]
    let ``all default viewmodels should be registered``() =

        ContainerBuilder()
        |> also registerModule<ViewModelsModule>
        |> also registerMocks
        |> build
        |> also Should.resolve<FaveViewModel>
        |> also Should.resolve<FeedViewModel>
        |> also Should.resolve<SearchViewModel>
        |> also Should.resolve<SettingsViewModel>
        |> also Should.resolve<ChannelsViewModel>
        |> dispose    
            