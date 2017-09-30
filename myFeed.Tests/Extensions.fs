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

    /// Disposes IDisposable in func style. 
    let dispose (disposable: IDisposable) = 
        disposable.Dispose()

    /// Converts sequence to collection.    
    let collection (sequence: seq<'a>) = 
        sequence.ToList()

    /// Attaches handler to IEvent.
    let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) = 
        event.Add handler
    
    /// C#-like await operator.
    let await (task: Task<'a>) = 
        async {
            let! result = task |> Async.AwaitTask 
            return result 
        } |> Async.RunSynchronously

    /// C#-like task await operator.
    let awaitTask (task: Task) = 
        async {
            let! result = task |> Async.AwaitTask 
            return result 
        } |> Async.RunSynchronously

    /// "Tee" functions.
    let inline also f x = f x; x  

    /// First for structs.
    let fst struct (a, b) = a 

    /// Second for structs.
    let snd struct (a, b) = b

/// Dependency injection module.
module Dep =

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
        let mock = Mock<'a>()
        builder |> registerInstanceAs<'a> mock.Object |> ignore

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

/// FSharpy fluent assertions.
module Should =

    /// Ensures a is null.
    let beNull<'a> (a: 'a) = 
        Assert.Null(a)

    /// Ensures a is not null.
    let notBeNull<'a> (a: 'a) = 
        Assert.NotNull(a)

    /// Ensures a equals b.
    let equal<'a> (a: 'a) (b: 'a) = 
        Assert.Equal(a, b)
    
    /// Ensures a not equals b.
    let notEqual<'a> (a: 'a) (b: 'a) = 
        Assert.NotEqual(a, b)

    /// Ensures instance can be resolved from scope.
    let resolve<'a> = Dep.resolve<'a> >> notBeNull
