module myFeed.Tests.Stores.DatabaseConnectionTests

open Xunit
open LiteDB
open myFeed.Tests.Extensions
open myFeed.Services.Models
open System

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should insert category into database``() =

    let collection = connection.GetCollection<Category>()
    collection.Insert(Category(Title="Foo")) |> ignore

    let category = Seq.item 0 <| collection.FindAll()
    Should.notEqual Guid.Empty category.Id
    Should.equal "Foo" category.Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``ids should be unique each insertion``() =
    
    let collection = connection.GetCollection<Category>()
    collection.Insert(Category(Title="Foo")) |> ignore
    collection.Insert(Category(Title="Bar")) |> ignore

    let response = List.ofSeq <| collection.FindAll()
    Should.notEqual response.[0].Id response.[1].Id                    

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should insert categories with nested documents``() =    

    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Title="Foo")]))]))) |> ignore

    let category = Seq.item 0 <| collection.FindAll()
    Should.equal 1 category.Channels.Count
    Should.equal 1 category.Channels.[0].Articles.Count
    Should.equal "Foo" category.Channels.[0].Articles.[0].Title  

[<Fact>]
[<CleanUpCollection("Setting")>]
let ``should find setting entities using their keys``() =

    let collection = connection.GetCollection<Setting>()
    collection.Insert(Setting(Key="Foo", Value="Bar")) |> ignore

    let setting = collection.FindOne(fun x -> x.Key = "Foo")
    Should.equal "Foo" setting.Key
    Should.equal "Bar" setting.Value    

[<Fact>]
[<CleanUpCollection("Setting")>]
let ``should find setting entities by keys using Query``() =

    let collection = connection.GetCollection<Setting>()
    collection.Insert(Setting(Key="Foo", Value="Bar")) |> ignore

    let query = Query.EQ("$.Key", BsonValue("Foo"))
    let setting = collection.FindOne(query)
    Should.equal "Foo" setting.Key
    Should.equal "Bar" setting.Value

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should scan nested entities in category``() =

    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Title="Foo", Channels=toList
            ([Channel(Uri="http://foo.bar")]))) |> ignore

    let query = Query.EQ("$.Channels[*].Uri", BsonValue("http://foo.bar")) 
    let category = collection.FindOne(query)
    Should.equal "Foo" category.Title
    Should.equal "http://foo.bar" category.Channels.[0].Uri    

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should scan deeply nested entities in category``() =
                        
    let collection = connection.GetCollection<Category>()
    collection.EnsureIndex(fun x -> x.Id) |> ignore
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Title="Foo")]))]))) |> ignore      

    let query = Query.EQ("$.Channels[*].Articles[*].Title", BsonValue("Foo"))
    let category = collection.FindOne(query)
    Should.equal "Foo" category.Channels.[0].Articles.[0].Title   

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should find category by it's id``() =  

    let category = Category(Title="Foo")
    let collection = connection.GetCollection<Category>()
    collection.Insert(category) |> ignore

    let query = Query.EQ("$._id", BsonValue(category.Id))
    let response = collection.FindOne(query)
    Should.equal category.Id response.Id        
    Should.equal "Foo" response.Title                      

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should update nested entity in database``() = 
    
    // Replaces channel and returns count of replaces items.
    let update (channel: Channel) =
        let collection = connection.GetCollection<Category>()
        let query = Query.EQ("$.Channels[*]._id", BsonValue(channel.Id))
        let category = collection.FindOne(query)
        category.Channels.RemoveAll(fun x -> x.Id = channel.Id) |> ignore
        category.Channels.Add(channel)   
        collection.Update(category) |> ignore    

    let channel = Channel(Uri="Foo")
    let collection = connection.GetCollection<Category>()
    collection.Insert(Category(Channels=toList([channel]))) |> ignore

    channel.Uri <- "Bar"
    update channel 

    let response = Seq.item 0 <| collection.FindAll()
    Should.equal "Bar" response.Channels.[0].Uri

[<Fact>]
[<CleanUpCollection("Article")>]
let ``should perform bulk insert of entities set``() =    

    let collection = connection.GetCollection<Article>()
    collection.InsertBulk(
        [ Article(Title="Foo");
          Article(Title="Bar");
          Article(Title="Zoo") ]) |> ignore

    let articles = List.ofSeq <| collection.FindAll()      
    Should.notEqual articles.[0].Id articles.[1].Id
    Should.notEqual articles.[1].Id articles.[2].Id
    Should.notEqual articles.[2].Id articles.[1].Id   

[<Fact>]
[<CleanUpCollection("Setting")>]
let ``should perform search on entities using Query``() =

    let collection = connection.GetCollection<Setting>()
    collection.Insert(Setting(Key="Foo", Value="Important")) |> ignore
    
    let query = Query.EQ("$.Key", BsonValue("Foo"))
    let response = collection.FindOne(query)
    Should.equal "Important" response.Value
    Should.equal "Foo" response.Key

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should performs search on nested objects using Query``() =

    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Title="Foo")]))]))) |> ignore  

    let query = Query.EQ("$.Channels[*].Articles[*].Title", BsonValue("Foo"))
    let response = collection.FindOne(query)
    Should.equal "Foo" response.Channels.[0].Articles.[0].Title

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should performs search on nested objects by id using Query``() =

    let uniqueIdentifier = Guid.NewGuid()
    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Id=uniqueIdentifier)]))]))) |> ignore  

    let query = Query.EQ("$.Channels[*].Articles[*]._id", BsonValue(uniqueIdentifier))
    let response = collection.FindOne(query)
    Should.equal uniqueIdentifier response.Channels.[0].Articles.[0].Id

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should find nested entity by id and return its instance``() =

    let identifier = Guid.NewGuid()
    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Title="Foo", Id=identifier)]))]))) |> ignore  

    let query = Query.EQ("$.Channels[*].Articles[*]._id", BsonValue(identifier))
    collection.FindOne(query).Channels
    |> Seq.collect (fun x -> x.Articles)
    |> Seq.find (fun x -> x.Id = identifier)
    |> fun x -> x.Title
    |> Should.equal "Foo"

[<Fact>]
[<CleanUpCollection("Category")>]
let ``should set not null unique identifier for child nodes``() =

    let collection = connection.GetCollection<Category>()
    collection.Insert(
        Category(Channels=toList
            ([Channel(Articles=toList
                ([Article(Title="Foo")]))]))) |> ignore

    let response = Seq.item 0 <| collection.FindAll()
    Should.notEqual Guid.Empty response.Id
    Should.notEqual Guid.Empty response.Channels.[0].Id    
    Should.notEqual Guid.Empty response.Channels.[0].Articles.[0].Id      
