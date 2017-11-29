module myFeed.Tests.Services.RegexImageTests

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services.Implementations

[<Fact>]
let ``should extract first image url from plain text``() =

    let service = produce<RegexImageService> []
    "Foo <bla a='42'></bla> \n<img foo='bar' src='http://example.com' />"
    |> (service.ExtractImageUri >> Should.equal "http://example.com")            

[<Fact>]
let ``should return null if there are no images``() =

    let service = produce<RegexImageService> []
    "London is the capital of Great Britain"
    |> (service.ExtractImageUri >> Should.equal null)

[<Fact>]
let ``should return exactly first match from text``() =    

    let service = produce<RegexImageService> []
    "?<img src='http://bar.foo' /> <img src='http://foo.bar' />"
    |> (service.ExtractImageUri >> Should.equal "http://bar.foo")

[<Fact>]
let ``should return null if passed string is null``() =

    let service = produce<RegexImageService> []
    null |> (service.ExtractImageUri >> Should.equal null)   