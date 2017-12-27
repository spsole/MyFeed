module myFeed.Tests.Fixtures.DryIocFactory

open Xunit
open DryIoc
open myFeed.Interfaces
open myFeed.Services
open myFeed.Tests.Extensions

type Sample (mediation: IStateContainer) = 
    member __.Name = mediation.Pop<string>()

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("Resolution message")>]
let ``should inject parameters with given type`` (phrase: string) =

    use container = new Container()
    container.Register<Sample>()
    container.Register<IStateContainer, DryIocStateContainer>()

    let factory = produce<DryIocFactoryService> [container]
    let instance = factory.CreateInstance<Sample> phrase
    Should.equal phrase instance.Name
    