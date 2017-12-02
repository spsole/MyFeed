module myFeed.Tests.Fixtures.FavoriteStoreTests

open Xunit
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions

let private repository = LiteDbFavoriteStoreService connection

[<Theory>]
[<InlineData("Foo")>]
[<InlineData("Bar")>]
[<CleanUpCollection("Article")>]
let ``should insert favorite articles into database`` title =

    repository.InsertAsync(Article(Title=title)).Wait()
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal title response.[0].Title

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should initialize unique ids when inserting articles``() =

    repository.InsertAsync(Article()).Wait()
    repository.InsertAsync(Article()).Wait()
    
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.notEqual response.[0].Id response.[1].Id

[<Theory>]
[<InlineData("Foo", "Title with spaces")>]
[<InlineData("Title with spaces", "Foo")>]
[<CleanUpCollection("Article")>]
let ``should update articles in article collection`` before after =

    let article = Article(Title=before)
    repository.InsertAsync(article).Wait()
    article.Title <- after
    repository.UpdateAsync(article).Wait()    

    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal after response.[0].Title

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should remove articles from article collection``() =  

    let article = Article()
    repository.InsertAsync(article).Wait()
    repository.RemoveAsync(article).Wait()

    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal 0 response.Length
