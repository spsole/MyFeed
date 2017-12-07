module myFeed.Tests.Fixtures.FavoritesTests
 
open Xunit
open NSubstitute
open myFeed.Tests.Extensions
open myFeed.Services.Models
open myFeed.Services.Implementations
open myFeed.Services.Abstractions
 
[<Fact>]
let ``should update fave property when inserting``() =
            
    let service = produce<FavoriteService> []
    let article = Article(Fave=false)  
    service.InsertAsync(article).Wait()
    Should.equal article.Fave true
      
[<Fact>]
let ``should update fave property when removing``() =
    
    let service = produce<FavoriteService> []
    let article = Article(Fave=true)  
    service.RemoveAsync(article).Wait()
    Should.equal article.Fave false

[<Fact>]
let ``should insert and remove articles``() =
      
    let mutable deleted, inserted = 0, 0
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.When(fun x -> x.InsertAsync(Arg.Any<_>()) |> ignore)
             .Do(fun _ -> inserted <- inserted + 1)
    favorites.When(fun x -> x.RemoveAsync(Arg.Any<_>()) |> ignore)
             .Do(fun _ -> deleted <- deleted + 1)             
      
    let article = Article(Fave=false)
    let service = produce<FavoriteService> [favorites]
      
    service.InsertAsync(article).Wait()
    service.InsertAsync(article).Wait()
    service.RemoveAsync(article).Wait()
    service.RemoveAsync(article).Wait()
      
    Should.equal 1 deleted
    Should.equal 1 inserted
