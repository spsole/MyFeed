[<AutoOpen>]
module myFeed.Tests.Extensions.Domain   

open LiteDB
open DryIoc
open Xunit.Sdk
open System.IO
open Default
open myFeed.Services.Platform

/// Registers instance as abstraction.
let registerInstanceAs<'i> (instance: obj) (builder: Container) =
    builder.RegisterDelegate<'i>(fun _ -> instance :?> 'i)
    
/// Registers mock instance info builder.
let registerMock<'a when 'a: not struct> (builder: Container) =
    builder.RegisterDelegate<'a>(fun _ -> NSubstitute.Substitute.For<'a>())

/// Single instance LiteDatabase connection.
let connection = new LiteDatabase("MyFeed.db")

/// Injects mocks into container builder.
let registerMocks (builder: Container) =
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
        