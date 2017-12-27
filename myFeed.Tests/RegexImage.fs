module myFeed.Tests.Fixtures.RegexImage

open Xunit
open myFeed.Tests.Extensions
open myFeed.Services.Implementations

[<Theory>]
[<InlineData("<img foo='bar' src='http://example.com' />", "http://example.com")>]
[<InlineData("<bla a='42'></bla><img src='http://foo.bar'>", "http://foo.bar")>]
[<InlineData("Foo \n<img src='http://example.com' />", "http://example.com")>]
let ``should extract image url from plain text`` snippet result =

    let service = produce<RegexImageService> []
    let response = service.ExtractImageUri snippet
    Should.equal result response

[<Theory>]
[<InlineData("<sample><img ></sample>")>]
[<InlineData("<b></b><image /><another></another>")>]
[<InlineData("London is the capital of Great Britain!")>]
let ``should return null if there are no images`` snippet =

    let service = produce<RegexImageService> []
    let response = service.ExtractImageUri snippet
    Should.equal null response
    
[<Fact>]
let ``should return null if passed string is null``() =

    let service = produce<RegexImageService> []
    let response = service.ExtractImageUri null
    Should.equal null response

[<Theory>]
[<InlineData("<img src='http://f.boo'/><img src='http://faa.bo'/>", "http://f.boo")>]
[<InlineData("<img src='http://a.b'/><img src='nothing'>", "http://a.b")>]
[<InlineData("<img src='nothing'/><img src='http://a.b'>", "http://a.b")>]
let ``should extract first match from plaint text`` snippet result =    

    let service = produce<RegexImageService> []
    let response = service.ExtractImageUri snippet
    Should.equal result response
