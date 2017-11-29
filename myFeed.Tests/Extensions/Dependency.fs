[<AutoOpen>]
module myFeed.Tests.Extensions.Dependency

open Autofac
open Default
open System.Reflection

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
let build (builder: ContainerBuilder) = builder.Build()

/// Builds scope from a builder with additional registrations.
let buildWith (objects: seq<obj>) (builder: ContainerBuilder) =
    for object in objects do
        let abstraction = object.GetType().GetInterfaces().[0]
        builder.RegisterInstance(object).As(abstraction) 
        |> ignore
    build builder  