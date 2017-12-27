module myFeed.Tests.Fixtures.Opml

open Xunit
open NSubstitute
open myFeed.Models
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Tests.Extensions
open System.Threading.Tasks
open System.IO

[<Theory>]
[<InlineData("Bar", "Foo", "https://", "site.com", "/feed")>]
[<InlineData("Abc", "Def", "https://", "example.com", "/news")>]
[<InlineData("Yii", "Zoo", "http://", "sub.domained.web", "/rss")>]
let ``should be able to export opml feeds`` first second protocol domain path =

    let uri = sprintf "%s%s%s" protocol domain path
    let categories = Substitute.For<ICategoryManager>()
    categories.GetAllAsync().Returns(
        [ Category(Title=first, Channels=toList[| Channel(Uri=uri) |]);
          Category(Title=second) ] :> seq<_>
        |> Task.FromResult) 
        |> ignore

    let mutable opml = null
    let serializer = Substitute.For<ISerializationService>()
    serializer.When(fun x -> x.Serialize<Opml>(Arg.Any(), Arg.Any()) |> ignore)     
              .Do(fun x -> opml <- x.Arg<Opml>())

    let service = produce<DefaultOpmlService> [categories; serializer]
    let response = service.ExportOpmlFeedsAsync(new MemoryStream()).Result

    Should.equal true response
    Should.equal 2 opml.Body.Count
    Should.equal first opml.Body.[0].Title
    Should.equal second opml.Body.[1].Title
    Should.equal 1 opml.Body.[0].ChildOutlines.Count
    Should.equal domain opml.Body.[0].ChildOutlines.[0].Title
    Should.equal (protocol + domain) opml.Body.[0].ChildOutlines.[0].HtmlUrl
    Should.equal uri opml.Body.[0].ChildOutlines.[0].XmlUrl

[<Theory>]
[<InlineData("http://foo.bar/rss")>]
[<InlineData("https://buy.some.beer/feed")>]
[<InlineData("https://long-domain.any/news")>]
let ``should be able to import opml feeds`` url =    

    let serializer = Substitute.For<ISerializationService>()
    serializer.Deserialize<Opml>(Arg.Any()).Returns(
        Opml(Body=toList [ OpmlOutline(XmlUrl=url);
                           OpmlOutline(XmlUrl=url) ])) |> ignore

    let mutable category = null
    let categories = Substitute.For<ICategoryManager>()
    categories.When(fun x -> x.InsertAsync(Arg.Any()) |> ignore)
              .Do(fun x -> category <- x.Arg<Category>())

    let service = produce<DefaultOpmlService> [serializer; categories]
    let response = service.ImportOpmlFeedsAsync(new MemoryStream()).Result

    Should.equal true response
    Should.equal 2 category.Channels.Count 
    Should.equal url category.Channels.[0].Uri
    Should.equal true category.Channels.[0].Notify
    Should.equal true category.Channels.[1].Notify
    Should.equal url category.Channels.[1].Uri
    