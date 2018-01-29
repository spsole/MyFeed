module myFeed.Tests.Fixtures.DryIocFactory

open Xunit
open DryIoc
open myFeed.Interfaces
open myFeed.Services
open myFeed.Tests.Extensions
open System

type Sample (mediation: string) = 
    member __.Name = mediation

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("Resolution message")>]
let ``should inject parameters with given type`` (phrase: string) =

    use container = new Container()
    container.Register<Sample>()

    let factory = produce<DryIocFactoryService> [container]
    let creator = factory.Create<Func<string, Sample>>()
    let instance = creator.Invoke phrase
    Should.equal phrase instance.Name
    