module myFeed.Tests.Fixtures.FavoriteTests

open Xunit
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions

let private repository = produce<LiteFavoriteManager> [connection]

[<Theory>]
[<InlineData("Foo")>]
[<InlineData("Bar")>]
[<CleanUpCollection("Article")>]
let ``should insert favorite articles into database`` title =

    let article = Article(Title=title, Fave=false)
    repository.InsertAsync(article).Wait()
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal title response.[0].Title
    Should.equal article.Fave true

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should initialize unique ids when inserting articles``() =

    repository.InsertAsync(Article()).Wait()
    repository.InsertAsync(Article()).Wait()
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.notEqual response.[0].Id response.[1].Id

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should remove articles from article collection``() =  

    let article = Article()
    repository.InsertAsync(article).Wait()
    repository.RemoveAsync(article).Wait()
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal 0 response.Length
