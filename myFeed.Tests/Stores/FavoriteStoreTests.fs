module myFeed.Tests.Stores.FavoriteStoreTests

open Xunit
open myFeed.Services.Abstractions
open myFeed.Services.Implementations
open myFeed.Services.Models
open myFeed.Tests.Extensions

let private repository = LiteDbFavoriteStoreService connection

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should insert favorite articles into database``() =

    repository.InsertAsync(Article(Title="Foo")).Wait()
    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal "Foo" response.[0].Title

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

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should update articles in article collection``() =

    let article = Article(Title="Foo")
    repository.InsertAsync(article).Wait()

    article.Title <- "Bar"
    repository.UpdateAsync(article).Wait()    

    let response = List.ofSeq <| repository.GetAllAsync().Result
    Should.equal "Bar" response.[0].Title
