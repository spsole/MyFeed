namespace myFeed.Tests.Extensions

open System
open System.IO
open System.Linq
open System.Reflection

open Xunit
open Xunit.Sdk
open NSubstitute
open Autofac
open LiteDB

[<AutoOpen>]
module StandardLibraryExtensions =

    /// Converts sequence to list.
    let toList (sequence: seq<'a>) = sequence.ToList()

    /// Disposes disposable in functional style.
    let dispose (disposable: IDisposable) = disposable.Dispose()

    /// C#-like event subscription operator.
    let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) = event.Add handler

    /// Applies function to argument and continues.
    let inline also defun arguments = defun arguments; arguments

    /// Extracts first element from tuple.
    let fst struct (first, _) = first 

    /// Extracts second element from tuple.
    let snd struct (_, second) = second   

    /// Produces instance of an object with dependencies supplied.
    let produce<'T when 'T : not struct> (injectables: obj seq) =
        let substituteIfNone (parameterInfo: ParameterInfo) =
            let parameterType = parameterInfo.ParameterType
            injectables 
            |> Seq.tryFind (fun x -> 
                let objectType = x.GetType() 
                objectType = parameterType ||
                objectType.GetInterfaces()
                |> Seq.contains parameterType)
            |> function
            | Some some -> some
            | None -> Substitute.For([| parameterType |], null)

        let objectType = typeof<'T>
        let ctor = Seq.item 0 <| objectType.GetConstructors()
        ctor.GetParameters()
        |> Seq.map substituteIfNone
        |> Seq.cast<obj>
        |> Array.ofSeq
        |> ctor.Invoke :?> 'T         

/// Fluent assertions.
module Should =

    /// Asserts that a is nothing.
    let beNull<'a> (a: 'a) = Assert.Null(a)

    /// Asserts that a is not nothing.
    let notBeNull<'a> (a: 'a) = Assert.NotNull(a)

    /// Asserts that a equals b.
    let equal<'a> (a: 'a) (b: 'a) = Assert.Equal(a, b)
    
    /// Asserts that a does not equal b.
    let notEqual<'a> (a: 'a) (b: 'a) = Assert.NotEqual(a, b)

    /// Asserts that a string contains b substring.
    let contain (a: string) (b: string) = Assert.Contains(a, b)
        
    /// Asserts that particular method throws an exception of a given type.    
    let throw<'e when 'e :> exn> act = Assert.Throws<'e>(Action(act))   

    /// Asserts that action throws inner exception of certain type.
    let throwInner<'e when 'e :> exn> (action: unit -> unit) =
        try
            action()
            failwith "Failure! No errors occured."
        with
        | :? AggregateException as aggregateExn ->
            let innerExn = aggregateExn.InnerException 
            let innerExnType = innerExn.GetType()
            equal innerExnType typeof<'e>

    /// Ensures that sequence is not empty.    
    let notBeEmpty<'a, 'b when 'a :> seq<'b>> (ls: 'a) = Assert.NotEmpty   

    /// Ensures that particular component has been already registered.
    let resolve<'a> (scope: ILifetimeScope) = scope.Resolve<'a>() |> Assert.NotNull  

/// Dependency injection module.
module Dependency =

    /// Registers type as abstraction.
    let registerAs<'a, 'i> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().As<'i>() |> ignore

    /// Registers type as self.
    let registerAsSelf<'a> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().AsSelf() |> ignore  

    /// Registers instance as abstraction.
    let registerInstanceAs<'i> (instance: obj) (builder: ContainerBuilder) =
        builder.RegisterInstance(instance).As<'i>() |> ignore  

    /// Registers lambda as abstraction.
    let registerLambda<'a> (func: IComponentContext -> 'a) (builder: ContainerBuilder) =
        builder.Register(func).AsSelf() |> ignore

    /// Registers module into container builder.
    let registerModule<'m when 'm: (new: unit -> 'm) and 'm :> Core.IModule> (builder: ContainerBuilder) =
        builder.RegisterModule<'m>() |> ignore     

    /// Registers mock instance info builder.
    let registerMock<'a when 'a: not struct> (builder: ContainerBuilder) =
        let mockInstance = NSubstitute.Substitute.For<'a>()
        builder |> registerInstanceAs<'a> mockInstance |> ignore

    /// Resolves stuff from scope.
    let resolve<'i> (scope: ILifetimeScope) = 
        scope.Resolve<'i>() 

    /// Resolves something from scope and disposes it.
    let resolveOnce<'i> (scope: ILifetimeScope) =
        let instance = resolve<'i> scope
        dispose scope; instance    

    /// Builds scope from a builder.        
    let buildScope (builder: ContainerBuilder) = 
        builder.Build()

/// Functions for myFeed domain model.
module Domain =   

    open Dependency
    open myFeed.Services
    open myFeed.Services.Platform
    open myFeed.Repositories
    open myFeed.ViewModels       

    /// Single instance LiteDatabase connection.
    let connection = new LiteDatabase("Database.db")

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
        |> ignore

    /// Creates builder for integration testing.
    let createBuilderForIntegrationTesting() =
        ContainerBuilder()  
        |> also registerModule<RepositoriesModule>
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