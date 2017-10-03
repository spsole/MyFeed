namespace myFeed.Tests.Modules

open Xunit
open Autofac
open System

open myFeed.Entities

open myFeed.Repositories
open myFeed.Repositories.Abstractions

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dep

open myFeed.Services
open myFeed.Services.Abstractions

open myFeed.ViewModels.Implementations
open myFeed.ViewModels

/// Tests for module builders.
type RegistrationsFixture() =    
    
    let registerMocks builder =
        builder
        |> also registerMock<IPlatformService>
        |> also registerMock<ITranslationsService>
        |> also registerMock<IFilePickerService>
        |> also registerMock<IDialogService>
        |> also registerMock<IDefaultsService>
        |> also registerMock<INavigationService>
        |> ignore

    [<Fact>]
    member x.``all data providers should be registered``() =
        ContainerBuilder() 
        |> also registerModule<RepositoriesModule>
        |> buildScope
        |> also Should.resolve<IArticlesRepository>
        |> also Should.resolve<ISourcesRepository>
        |> also Should.resolve<IConfigurationRepository>
        |> dispose

    [<Fact>]
    member x.``all default services should be registered``() = 
        ContainerBuilder()
        |> also registerModule<ServicesModule> 
        |> also registerMocks
        |> buildScope
        |> also Should.resolve<ISourcesRepository>
        |> also Should.resolve<IConfigurationRepository>
        |> also Should.resolve<IArticlesRepository>
        |> also Should.resolve<ISearchService>
        |> also Should.resolve<IOpmlService>
        |> also Should.resolve<ISerializationService>
        |> also Should.resolve<IExtractImageService>
        |> also Should.resolve<IFeedFetchService>
        |> also Should.resolve<IFeedStoreService>
        |> also Should.resolve<ISettingsService>
        |> dispose

    [<Fact>]
    member x.``all default viewmodels should be registered``() =
        ContainerBuilder()
        |> also registerModule<ViewModelsModule>
        |> also registerMocks
        |> buildScope
        |> also Should.resolve<FaveViewModel>
        |> also Should.resolve<FeedViewModel>
        |> also Should.resolve<SearchViewModel>
        |> also Should.resolve<SettingsViewModel>
        |> also Should.resolve<SourcesViewModel>
        |> dispose    