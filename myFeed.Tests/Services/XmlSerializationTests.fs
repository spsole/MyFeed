module myFeed.Tests.Services.XmlSerializationTests

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services.Implementations
open myFeed.Services.Models
open System.IO

[<Fact; CleanUpFile("sample")>]
let ``should serialize typed objects into xml``() =

    let service = produce<XmlSerializationService> []
    let instance = Opml(Head=OpmlHead(Title="Foo"))
    service.Serialize<Opml>(instance, File.OpenWrite "sample")
    Should.contain "Foo" (File.ReadAllText "sample")

[<Fact; CleanUpFile("sample")>]
let ``should deserialize typed objects from xml``() =

    let service = produce<XmlSerializationService> []
    let instance = Opml(Head=OpmlHead(Title="Bar"))
    service.Serialize<Opml>(instance, File.OpenWrite "sample")
    let opml = service.Deserialize<Opml>(File.OpenRead "sample")
    Should.equal "Bar" opml.Head.Title
    