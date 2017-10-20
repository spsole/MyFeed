namespace myFeed.Tests.Repositories

open Xunit
open LiteDB

open System

open myFeed.Repositories.Models
open myFeed.Repositories.Implementations

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Domain

module FavoritesRepositoryFixture =

    let private repository = FavoritesRepository <| connection

    [<Fact; CleanUpCollection("Article")>]
    let ``should insert favorite articles into database``() =

        repository.InsertAsync(Article(Title="Foo")).Wait()
        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal "Foo" response.[0].Title

    [<Fact; CleanUpCollection("Article")>]
    let ``should initialize unique ids when inserting articles``() =

        repository.InsertAsync(Article()).Wait()
        repository.InsertAsync(Article()).Wait()
        
        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.notEqual response.[0].Id response.[1].Id

    [<Fact; CleanUpCollection("Article")>]
    let ``should remove articles from article collection``() =  

        let article = Article()
        repository.InsertAsync(article).Wait()
        repository.RemoveAsync(article).Wait()

        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal 0 response.Length

    [<Fact; CleanUpCollection("Article")>]
    let ``should update articles in article collection``() =

        let article = Article(Title="Foo")
        repository.InsertAsync(article).Wait()

        article.Title <- "Bar"
        repository.UpdateAsync(article).Wait()    

        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal "Bar" response.[0].Title

module SettingsRepositoryFixture =

    let private repository = SettingsRepository <| connection

    [<Fact; CleanUpCollection("Setting")>]
    let ``should insert settings into database``() =    

        repository.InsertAsync(Setting(Key="Foo", Value="Bar")).Wait()

        let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
        Should.equal "Bar" response.[0].Value
        Should.equal "Foo" response.[0].Key

    [<Fact; CleanUpCollection("Setting")>]
    let ``should return settings by their keys``() =

        repository.InsertAsync(Setting(Key="Foo", Value="Bar")).Wait()
        let setting = repository.GetByKeyAsync("Foo").Result   
        Should.equal "Bar" setting.Value   

    [<Fact; CleanUpCollection("Setting")>]   
    let ``should update settings in database``() =

        let setting = Setting(Key="Bar", Value="Foo")        
        repository.InsertAsync(setting).Wait()

        setting.Key <- "Foo"
        setting.Value <- "Bar"
        repository.UpdateAsync(setting).Wait()

        let response = List.ofSeq <| connection.GetCollection<Setting>().FindAll()
        Should.equal "Foo" response.[0].Key
        Should.equal "Bar" response.[0].Value

module CategoriesRepositoryFixture =

    let private repository = CategoriesRepository <| connection

    [<Fact; CleanUpCollection("Category")>]
    let ``should insert categories into categories repository``() =

        repository.InsertAsync(Category(Title="Foo")).Wait()
        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal "Foo" response.[0].Title

    [<Fact; CleanUpCollection("Category")>]
    let ``should order categories when inserting ones``() =    

        repository.InsertAsync(Category(Title="Foo")).Wait()
        repository.InsertAsync(Category(Title="Bar")).Wait()
        repository.InsertAsync(Category(Title="Zoo")).Wait()
        
        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal "Foo" response.[0].Title
        Should.equal "Bar" response.[1].Title
        Should.equal "Zoo" response.[2].Title
        Should.equal 0 response.[0].Order
        Should.equal 1 response.[1].Order
        Should.equal 2 response.[2].Order

    [<Fact; CleanUpCollection("Category")>]
    let ``should remove categories from database``() =

        let category = Category()
        repository.InsertAsync(category).Wait()
        repository.RemoveAsync(category).Wait()
        
        repository.GetAllAsync().Result
        |> Seq.length
        |> Should.equal 0   

    [<Fact; CleanUpCollection("Category")>]
    let ``should update inserted categories in database``() =

        let category = Category(Title="Foo")
        repository.InsertAsync(category).Wait()

        category.Title <- "Bar"  
        repository.UpdateAsync(category).Wait()

        let category = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal "Bar" category.Title

    [<Fact; CleanUpCollection("Category")>]
    let ``should find article across all collection using its id``() =

        let identifier = Guid.NewGuid()
        repository.InsertAsync(
            Category(Channels=toList
                ([Channel(Articles=toList
                    ([Article(Id=identifier, 
                        Title="Secret")]))]))).Wait() |> ignore

        let article = repository.GetArticleByIdAsync(identifier).Result
        Should.equal identifier article.Id
        Should.equal "Secret" article.Title     

    [<Fact; CleanUpCollection("Category")>]
    let ``should return null if no articles exist for given id``() =

        repository.GetArticleByIdAsync(Guid.NewGuid()).Result
        |> Should.equal null    

    [<Fact; CleanUpCollection("Category")>]
    let ``should change categories order based on sequence order``() =

        let categories =
            [ Category(Title="One");
              Category(Title="Two");
              Category(Title="Three") ]               

        for category in categories do 
            repository.InsertAsync(category).Wait()

        [ categories.[2];
          categories.[0];
          categories.[1] ]
        |> repository.RearrangeAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously

        let response = List.ofSeq <| repository.GetAllAsync().Result  
        Should.equal "Three" response.[0].Title
        Should.equal "One" response.[1].Title
        Should.equal "Two" response.[2].Title

    [<Fact; CleanUpCollection("Category")>]
    let ``should insert channel into existing category``() =    

        let category = Category()
        let channel = Channel(Uri="Foo")
        repository.InsertAsync(category).Wait()
        repository.InsertChannelAsync(category, channel).Wait()

        let response = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal 1 response.Channels.Count
        Should.equal "Foo" response.Channels.[0].Uri
   
    [<Fact; CleanUpCollection("Category")>]
    let ``should remove channel from existing category``() = 
   
        let channel = Channel(Uri="Foo")
        let category = Category(Channels=toList([channel]))
        repository.InsertAsync(category).Wait()
        repository.RemoveChannelAsync(category, channel).Wait()

        let response = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal 0 response.Channels.Count

    [<Fact; CleanUpCollection("Category")>]
    let ``should update channel existing in database``() = 
   
        let channel = Channel(Uri="Foo", Notify=false)
        repository.InsertAsync(Category(Channels=toList([channel]))).Wait()

        channel.Notify <- true
        channel.Uri <- "Bar"
        repository.UpdateChannelAsync(channel).Wait()

        let response = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal true response.Channels.[0].Notify
        Should.equal "Bar" response.Channels.[0].Uri

    [<Fact; CleanUpCollection("Category")>]
    let ``should insert articles into database``() =    

        let channel = Channel()
        repository.InsertAsync(Category(Channels=toList([channel]))).Wait()
        repository.InsertArticleRangeAsync(channel, 
            [ Article(Title="Foo") ]).Wait()

        let category = Seq.item 0 <| repository.GetAllAsync().Result      
        Should.equal "Foo" category.Channels.[0].Articles.[0].Title

    [<Fact; CleanUpCollection("Category")>]
    let ``should remove articles range from database``() =    

        let channel = Channel(Articles=toList([ Article(Title="Bar") ]))
        repository.InsertAsync(Category(Channels=toList([channel]))).Wait()        
        repository.RemoveArticleRangeAsync(channel, channel.Articles).Wait()

        let category = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal 0 category.Channels.[0].Articles.Count

    [<Fact; CleanUpCollection("Category")>]
    let ``should update existing articles in database``() =

        let article = Article(Title="Bar")
        repository.InsertAsync(
            Category(Channels=toList
                ([Channel(Articles=toList
                    ([article]))]))).Wait()

        article.Title <- "Foo"
        repository.UpdateArticleAsync(article).Wait()
        
        let category = Seq.item 0 <| repository.GetAllAsync().Result
        Should.equal "Foo" category.Channels.[0].Articles.[0].Title 

    [<Fact; CleanUpCollection("Category")>]
    let ``should fail with exception if no article exists``() =

        let article = Article(Title="Foo")
        fun () -> repository.UpdateArticleAsync(article).Wait()
        |> (Should.throwInner<ArgumentOutOfRangeException> >> ignore)

        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal 0 response.Length

    [<Fact; CleanUpCollection("Category")>]
    let ``should fail with exception if no channel exists``() =    

        let channel = Channel(Uri="Foo")  
        fun () -> repository.UpdateChannelAsync(channel).Wait()
        |> (Should.throwInner<ArgumentOutOfRangeException> >> ignore)

        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal 0 response.Length
    
    [<Fact; CleanUpCollection("Category")>]
    let ``should not have null ids on nested entites``() =

        repository.InsertAsync(
            Category(Channels=toList
                [Channel(Articles=toList
                    [Article()])])).Wait()

        let response = List.ofSeq <| repository.GetAllAsync().Result
        Should.notEqual Guid.Empty response.[0].Id                
        Should.notEqual Guid.Empty response.[0].Channels.[0].Id             
        Should.notEqual Guid.Empty response.[0].Channels.[0].Articles.[0].Id        
        