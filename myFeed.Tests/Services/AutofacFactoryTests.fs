module myFeed.Tests.Services.AutofacFactoryTests

open Xunit
open Autofac
open myFeed.Services.Implementations
open myFeed.Tests.Extensions

type Sample (name: string) = member __.Name = name

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("Resolution message")>]
let ``should inject parameters with given type`` (phrase: string) =

    let containerBuilder = ContainerBuilder()
    containerBuilder.RegisterType<Sample>().AsSelf() |> ignore
    let lifetimeScope = containerBuilder.Build()

    let factory = produce<AutofacFactoryService> [lifetimeScope]
    let instance = factory.CreateInstance<Sample> phrase
    Should.equal phrase instance.Name
    