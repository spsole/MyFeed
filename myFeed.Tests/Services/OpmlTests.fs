module myFeed.Tests.Services.OpmlTests

open Xunit
open NSubstitute
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions
open System.Linq
open System.IO
open System.Threading.Tasks

[<Fact>]
let ``should be able to export opml feeds``() =

    let categories = Substitute.For<ICategoryStoreService>()
    categories.GetAllAsync().Returns(
        [ Category(Title="Foo"); 
          Category(Title="Bar", Channels=toList
            [| Channel(Uri="http://example.com/rss") |]) ]
        |> fun seq -> seq.OrderBy(fun i -> i.Title)
        |> Task.FromResult) 
        |> ignore

    let mutable opml = null
    let serializer = Substitute.For<ISerializationService>()
    serializer.When(fun x -> x.Serialize<Opml>(Arg.Any(), Arg.Any()) |> ignore)     
              .Do(fun x -> opml <- x.Arg<Opml>())

    let service = produce<OpmlService> [categories; serializer]
    let response = service.ExportOpmlFeedsAsync(new MemoryStream()).Result

    Should.equal true response
    Should.equal 2 opml.Body.Count
    Should.equal "Bar" opml.Body.[0].Title
    Should.equal "Foo" opml.Body.[1].Title
    Should.equal 1 opml.Body.[0].ChildOutlines.Count
    Should.equal "example.com" opml.Body.[0].ChildOutlines.[0].Title
    Should.equal "http://example.com" opml.Body.[0].ChildOutlines.[0].HtmlUrl
    Should.equal "http://example.com/rss" opml.Body.[0].ChildOutlines.[0].XmlUrl

[<Fact>]
let ``should be able to import opml feeds``() =    

    let serializer = Substitute.For<ISerializationService>()
    serializer.Deserialize<Opml>(Arg.Any()).Returns(
        Opml(Body=toList
            [ OpmlOutline(XmlUrl="http://foo.com");
              OpmlOutline(XmlUrl="https://bar.com") ])) 
        |> ignore

    let mutable category = null
    let categories = Substitute.For<ICategoryStoreService>()
    categories.When(fun x -> x.InsertAsync(Arg.Any()) |> ignore)
              .Do(fun x -> category <- x.Arg<Category>())

    let service = produce<OpmlService> [serializer; categories]
    let response = service.ImportOpmlFeedsAsync(new MemoryStream()).Result

    Should.equal true response
    Should.equal 2 category.Channels.Count 
    Should.equal "http://foo.com" category.Channels.[0].Uri
    Should.equal "https://bar.com" category.Channels.[1].Uri
    