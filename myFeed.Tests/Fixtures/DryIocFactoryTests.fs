module myFeed.Tests.Fixtures.DryIocFactoryTests

open Xunit
open DryIoc
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Tests.Extensions

type Sample (mediation: IMediationService) = 
    member __.Name = mediation.Pop<string>()

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("Resolution message")>]
let ``should inject parameters with given type`` (phrase: string) =

    use container = new Container()
    container.Register<Sample>()
    container.Register<IMediationService, MediationService>()

    let factory = produce<DryIocFactoryService> [container]
    let instance = factory.CreateInstance<Sample> phrase
    Should.equal phrase instance.Name
    