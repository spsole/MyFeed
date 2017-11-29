module myFeed.Tests.Services.FavoritesTests
 
open Xunit
open NSubstitute
open myFeed.Tests.Extensions
open myFeed.Services.Models
open myFeed.Services.Implementations
open myFeed.Services.Abstractions
 
[<Fact>]
let ``should update article entity fave value when adding and removing``() =
      
    let article = Article(Fave=false)        
    let service = produce<FavoriteService> []
     
    service.Insert(article).Wait()
    Should.equal article.Fave true
      
    service.Remove(article).Wait()
    Should.equal article.Fave false

[<Fact>]
let ``should insert and remove article via repository``() =
      
    let mutable deleted = 0
    let mutable inserted = 0
     
    let favorites = Substitute.For<IFavoriteStoreService>()
    favorites.When(fun x -> x.InsertAsync(Arg.Any<_>()) |> ignore)
             .Do(fun _ -> inserted <- inserted + 1)
    favorites.When(fun x -> x.RemoveAsync(Arg.Any<_>()) |> ignore)
             .Do(fun _ -> deleted <- deleted + 1)             
      
    let article = Article(Fave=false)
    let service = produce<FavoriteService> [favorites]
      
    service.Insert(article).Wait()
    service.Insert(article).Wait()
    service.Remove(article).Wait()
    service.Remove(article).Wait()
      
    Should.equal 1 deleted
    Should.equal 1 inserted
