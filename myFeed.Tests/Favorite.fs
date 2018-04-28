module myFeed.Tests.Fixtures.Favorite

open Xunit
open myFeed.Models
open myFeed.Services
open myFeed.Tests.Extensions

let private repository = produce<LiteFavoriteManager> [connection]

[<Theory>]
[<InlineData("Foo")>]
[<InlineData("Bar")>]
[<CleanUpCollection("Article")>]
let ``should insert favorite articles into database`` title =

    let article = Article(Title=title, Fave=false)
    repository.Insert(article).Wait()
    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal title response.[0].Title
    Should.equal article.Fave true

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should initialize unique ids when inserting articles``() =

    repository.Insert(Article()).Wait()
    repository.Insert(Article()).Wait()
    let response = List.ofSeq <| repository.GetAll().Result
    Should.notEqual response.[0].Id response.[1].Id

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should remove articles from article collection``() =  

    let article = Article()
    repository.Insert(article).Wait()
    repository.Remove(article).Wait()
    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal 0 response.Length
    