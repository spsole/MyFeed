module myFeed.Tests.LiteCategoryManager

open Xunit
open myFeed.Tests
open myFeed.Services
open myFeed.Models
open System

let private repository = produce<LiteCategoryManager> [connection]

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should insert categories into categories repository``() =

    repository.Insert(Category(Title="Foo")).Wait()
    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal "Foo" response.[0].Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should order categories when inserting ones``() =    

    repository.Insert(Category(Title="Foo")).Wait()
    repository.Insert(Category(Title="Bar")).Wait()
    repository.Insert(Category(Title="Zoo")).Wait()
    
    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal "Foo" response.[0].Title
    Should.equal "Bar" response.[1].Title
    Should.equal "Zoo" response.[2].Title
    Should.equal 0 response.[0].Order
    Should.equal 1 response.[1].Order
    Should.equal 2 response.[2].Order

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should remove categories from database``() =

    let category = Category()
    repository.Insert(category).Wait()
    repository.Remove(category).Wait()

    repository.GetAll().Result
    |> Seq.length
    |> Should.equal 0   

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should update inserted categories in database``() =

    let category = Category(Title="Foo")
    repository.Insert(category).Wait()

    category.Title <- "Bar"  
    repository.Update(category).Wait()

    let category = Seq.item 0 <| repository.GetAll().Result
    Should.equal "Bar" category.Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should find article across all collection using its id``() =

    let identifier = Guid.NewGuid()
    repository.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Id=identifier, 
                    Title="Secret")]))]))).Wait() |> ignore

    let article = repository.GetArticleById(identifier).Result
    Should.equal identifier article.Id
    Should.equal "Secret" article.Title     

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should return null if no articles exist for given id``() =

    repository.GetArticleById(Guid.NewGuid()).Result
    |> Should.equal null    

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should change categories order based on sequence order``() =

    let categories =
        [ Category(Title="One");
          Category(Title="Two");
          Category(Title="Three") ]               

    for category in categories do 
        repository.Insert(category).Wait()

    [ categories.[2];
      categories.[0];
      categories.[1] ]
    |> repository.Rearrange
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore

    let response = List.ofSeq <| repository.GetAll().Result  
    Should.equal "Three" response.[0].Title
    Should.equal "One" response.[1].Title
    Should.equal "Two" response.[2].Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should insert channel into existing category``() =    

    let category = Category()
    let channel = Channel(Uri="Foo")
    repository.Insert(category).Wait()
    category.Channels.Add channel
    repository.Update(category).Wait()

    let response = Seq.item 0 <| repository.GetAll().Result
    Should.equal 1 response.Channels.Count
    Should.equal "Foo" response.Channels.[0].Uri

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should remove channel from existing category``() = 

    let channel = Channel(Uri="Foo")
    let category = Category(Channels=toList([channel]))
    repository.Insert(category).Wait()
    category.Channels.Remove channel |> ignore
    repository.Update(category).Wait()

    let response = Seq.item 0 <| repository.GetAll().Result
    Should.equal 0 response.Channels.Count

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should update channel existing in database``() = 

    let channel = Channel(Uri="Foo", Notify=false)
    repository.Insert(Category(Channels=toList([channel]))).Wait()

    channel.Notify <- true
    channel.Uri <- "Bar"
    repository.Update(channel).Wait()

    let response = Seq.item 0 <| repository.GetAll().Result
    Should.equal true response.Channels.[0].Notify
    Should.equal "Bar" response.Channels.[0].Uri

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should insert articles into database``() =    

    let channel = Channel()
    repository.Insert(Category(Channels=toList([channel]))).Wait()
    channel.Articles.AddRange [ Article(Title="Foo") ]
    repository.Update(channel).Wait()

    let category = Seq.item 0 <| repository.GetAll().Result      
    Should.equal "Foo" category.Channels.[0].Articles.[0].Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should update existing articles in database``() =

    let article = Article(Title="Bar")
    repository.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([article]))]))).Wait()

    article.Title <- "Foo"
    repository.Update(article).Wait()
    
    let category = Seq.item 0 <| repository.GetAll().Result
    Should.equal "Foo" category.Channels.[0].Articles.[0].Title 

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should not fail with exception if no article exists``() =

    let article = Article(Title="Foo")
    repository.Update(article).Wait()

    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal 0 response.Length

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should not fail with exception if no channel exists``() =    

    let channel = Channel(Uri="Foo")  
    repository.Update(channel).Wait()

    let response = List.ofSeq <| repository.GetAll().Result
    Should.equal 0 response.Length

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should not have null ids on nested entites``() =

    repository.Insert(
        Category(Channels=toList
            [Channel(Articles=toList
                [Article()])])).Wait()

    let response = List.ofSeq <| repository.GetAll().Result
    Should.notEqual Guid.Empty response.[0].Id                
    Should.notEqual Guid.Empty response.[0].Channels.[0].Id             
    Should.notEqual Guid.Empty response.[0].Channels.[0].Articles.[0].Id        
    