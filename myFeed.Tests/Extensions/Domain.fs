[<AutoOpen>]
module myFeed.Tests.Extensions.Domain   

open LiteDB
open Autofac
open Xunit.Sdk
open System.IO
open Default
open Dependency
open myFeed.Services.Platform
open myFeed.ViewModels.Implementations
open myFeed.Services
open myFeed.ViewModels

/// Single instance LiteDatabase connection.
let connection = new LiteDatabase("MyFeed.db")

/// Injects mocks into container builder.
let registerMocks (builder: ContainerBuilder) =
    builder 
    |> also (registerInstanceAs<LiteDatabase> (new LiteDatabase("_.db")))
    |> also registerMock<ITranslationsService>
    |> also registerMock<IFilePickerService>       
    |> also registerMock<IPlatformService>
    |> also registerMock<IDialogService>
    |> also registerMock<INavigationService>
    |> also registerMock<INotificationService>
    |> also registerMock<IPackagingService>
    |> ignore

/// Creates builder for integration testing.
let createBuilderForIntegrationTesting() =
    ContainerBuilder()  
    |> also registerModule<ServicesModule>
    |> also registerModule<ViewModelsModule> 
    |> also registerMocks

/// Attribute that cleans database up before and after test.
type CleanUpCollectionAttribute (name: string) =
    inherit BeforeAfterTestAttribute() 
    override __.Before _ = connection.DropCollection name |> ignore
    override __.After _ = connection.DropCollection name |> ignore

/// Deletes the entire file from disk to prevent collisions.
type CleanUpFileAttribute (name: string) =
    inherit BeforeAfterTestAttribute()
    override __.Before _ = File.Delete name      
    override __.After _ = File.Delete name     
        