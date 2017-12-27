module myFeed.Tests.Fixtures.XmlSerialization

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services
open myFeed.Models
open System.IO

[<Theory>]
[<InlineData("Foo")>]
[<InlineData("Longer phrase")>]
[<CleanUpFile("sample")>]
let ``should serialize typed objects into xml`` value =

    let file = "sample"
    let service = produce<XmlSerializationService> []
    let instance = Opml(Head=OpmlHead(Title=value))
    service.Serialize<Opml>(instance, File.OpenWrite file)
    Should.contain value (File.ReadAllText file)

[<Theory>]
[<InlineData("Bar")>]
[<InlineData("Another phrase")>]
[<CleanUpFile("sample")>]
let ``should deserialize typed objects from xml`` value =

    let file = "sample"
    let service = produce<XmlSerializationService> []
    let instance = Opml(Head=OpmlHead(Title=value))
    service.Serialize<Opml>(instance, File.OpenWrite file)
    let opml = service.Deserialize<Opml>(File.OpenRead file)
    Should.equal value opml.Head.Title
    