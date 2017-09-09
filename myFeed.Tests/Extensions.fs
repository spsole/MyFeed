namespace myFeed.Tests.Extensions

open System
open System.Linq
open System.Reflection
open System.Threading.Tasks
open System.Collections.Generic

open Autofac
open Xunit
open Moq

[<AutoOpen>]
module Tools =

    // Curried dispose.
    let dispose (disposable: IDisposable) =
        disposable.Dispose()
    
    /// Converts seq to List/ICollection/Etc.
    let collection (sequence: seq<'a>) =
        sequence.ToList()
    
    /// Await pattern from C#.
    let await (task: Task<'a>) = 
        async {
            let! result = task |> Async.AwaitTask 
            return result 
        } |> Async.RunSynchronously

    /// Await pattern from C# for simple tasks.
    let awaitTask (task: Task) = 
        async {
            let! result = task |> Async.AwaitTask 
            return result 
        } |> Async.RunSynchronously

    /// Pipe for asynchronious operations.
    let inline (|@>) t f = t |> await |> f 

    /// Function for better piping and chaining support.
    let tee f x = f <| x; x  

    /// Subscribes ('b -> unit) lambda to event.
    let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) =
        event.Add handler

    /// Invokes action adn returns completed task.
    let actionAsAwaitableFunc (action: unit -> unit) =
        Func<Task>(fun () -> 
            action()
            Task.CompletedTask)

    /// Awaits generic task and returns non generic one.
    let taskAsAwaitableFunc (task: Task<_>) =
        task |@> ignore
        Func<Task>(fun () -> Task.CompletedTask)    

module DependencyInjection =

    /// Registers types for Autofac IoC container.
    let registerAs<'a, 'i> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().As<'i>() |> ignore

    /// Registers type as self.
    let registerAsSelf<'a> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().AsSelf() |> ignore  

    /// Registers instance to interface.
    let registerInstanceAs<'i> (instance: obj) (builder: ContainerBuilder) =
        builder.RegisterInstance(instance).As<'i>() |> ignore  

    /// Registers lambda instantiation as self.
    let registerLambda<'a> (func: IComponentContext -> 'a) (builder: ContainerBuilder) =
        builder.Register(func).AsSelf() |> ignore

    /// Registers autofac module.
    let registerModule<'m when 'm: (new: unit -> 'm) and 'm :> Core.IModule> (builder: ContainerBuilder) =
        builder.RegisterModule<'m>() |> ignore     

    /// Resolves abstraction from autofac lifetime scope.
    let resolve<'i> (scope: ILifetimeScope) = 
        scope.Resolve<'i>() 

    /// Tries to resolve and item and checks it for null.
    let assertResolve<'i> (scope: ILifetimeScope) =
        scope |> resolve<'i> |> Assert.NotNull

    /// Builds and starts lifetime scope for builder.
    let buildScope (builder: ContainerBuilder) =
        builder.Build()   

module Mocking =
    open DependencyInjection

    /// Creates dummy object implementing interface.
    let getMock<'a when 'a: not struct>() = 
        Mock<'a>().Object

    /// Registers mock instance.
    let registerMockInstance<'a when 'a: not struct> (builder: ContainerBuilder) =
        builder |> registerInstanceAs<'a> (getMock<'a>()) |> ignore
    