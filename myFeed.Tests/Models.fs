namespace myFeed.Tests.Models

open Xunit

open System
open System.Linq

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging

open myFeed.Repositories.Entities
open myFeed.Repositories.Entities.Local
open myFeed.Repositories.Abstractions

open myFeed.Tests.Extensions
open myFeed.Tests.Logging

[<AutoOpen>]
module ModelsHelpers =         

    // Saves changes to hard disk.
    let save (context: DbContext) = 
        context.SaveChanges() |> ignore

    // Counts elements in DbSet.
    let count<'D when 'D: not struct> (set: DbSet<'D>) =
        set.CountAsync() |> await

    // Clears given DbSet.
    let clear<'D when 'D: not struct> (context: DbContext) (set: DbSet<'D>) =
        set |> set.RemoveRange
        save context |> ignore

    // Migrates database if it's not migrated.
    let migrate (context: DbContext) =
        if (not <| context.Database.GetAppliedMigrations().Any()) then
            context.Database.MigrateAsync() |> awaitTask   

    // Builds context using LoggerFactory.
    let buildLoggableContext() =
        let factory = new LoggerFactory()
        factory.AddProvider <| new XUnitLoggerProvider()
        new EntityContext(factory)
        |> tee migrate 

    // Builds raw models context.
    let buildContext() =
        new EntityContext()
        |> tee migrate 

/// Tests related to sources tables relations
/// under EntityFrameworkCore database.
module SourcesTableTests = 

    let context = buildContext()
    let set = context.SourceCategories

    // Populates context with sample entities.
    let populate() = 
        let first =  
            [ SourceEntity(Uri="foo", Notify=true);
              SourceEntity(Uri="bar", Notify=false) ]
            |> collection          
        let second = 
            [ SourceEntity(Uri="foobar", Notify=false) ]
            |> collection
        [ SourceCategoryEntity(Title="Foo", Sources=first);
          SourceCategoryEntity(Title="Bar", Sources=second) ]
        |> set.AddRange
        save context

    [<Fact; Log>]
    let ``should populate table with items``() =
        Assert.Equal(0, count set)
        populate()
        Assert.Equal(2, count set)
        clear context set

    [<Fact; Log>]
    let ``should be able to select items from table``() =
        Assert.Equal(0, count set)
        populate()

        let category = 
            fun (i: SourceCategoryEntity) -> i.Title = "Foo"
            |> context.SourceCategories.FirstAsync
            |> await
        let sourcesWithShortUrisCount =
            category.Sources
            |> Seq.where (fun s -> s.Uri.Length = 3)
            |> Seq.length

        Assert.Equal(sourcesWithShortUrisCount, 2)
        clear context set

    [<Fact; Log>]
    let ``should be able to select items from multiple tables``() =
        Assert.Equal(0, count set)
        populate()

        fun (i: SourceCategoryEntity) -> 
            i.Title = "Bar" && 
            (i.Sources 
            |> Seq.item 0
            |> fun s -> not s.Notify)
        |> context.SourceCategories.FirstAsync
        |> await
        |> fun category -> Assert.Equal(1, category.Sources.Count)
        clear context set

/// Configuration table basic functionality tests.
module ConfigurationTableTests =

    let context = buildContext()
    let set = context.Configuration

    [<Fact; Log>]
    let ``should be able to insert and remove items``() =
        ConfigurationEntity(Key="", Value="")
        |> context.Configuration.Add
        |> ignore

        save context
        Assert.NotEqual(0, count set)
        clear context set

    [<Fact; Log>]
    let ``should select item by it's key``() =
        ConfigurationEntity(Key="Foo", Value="Bar")  
        |> context.Configuration.Add
        |> ignore
        save context 

        context.Configuration
        |> Seq.where (fun i -> i.Key = "Foo")
        |> Seq.item 0
        |> fun i -> Assert.Equal("Bar", i.Value)
        clear context set    

/// Tests related to articles table from myFeed
/// EntityFrameworkCore database.
module ArticlesTableTests =

    let context = buildContext()
    let set = context.Articles

    // Populates set with sample articles.
    let populate() =
        [ArticleEntity(Content="Foo", FeedTitle="Bar");
         ArticleEntity(Content="Foo", FeedTitle="Foo");
         ArticleEntity(Content="Bar", FeedTitle="Foobar")]
        |> set.AddRange       
        save context         

    [<Fact; Log>]
    let ``should be able to insert items``() =
        Assert.Equal(0, count set)
        
        let trackingEntity = 
            ArticleEntity() 
            |> tee (fun e -> e.Title <- "foobar")
            |> set.Add
        Assert.NotNull(trackingEntity)
        save context

        let first = await <| set.FirstAsync()
        Assert.Equal("foobar", first.Title)
        clear context set

    [<Fact; Log>]
    let ``should insert range``() =
        Assert.Equal(0, count set)

        let insertionsCount = 5
        fun i -> ArticleEntity()
        |> Seq.init<ArticleEntity> insertionsCount
        |> set.AddRange
        save context

        Assert.Equal(insertionsCount, count set)
        clear context set

    [<Fact; Log>]
    let ``should find item in database``() =
        Assert.Equal(0, count set)
        populate()

        context.Articles
        |> Seq.find (fun i -> i.Content = "Bar")
        |> fun found -> Assert.Equal(found.FeedTitle, "Foobar")
        clear context set

    [<Fact; Log>]
    let ``should select nessesary items from database``() =
        Assert.Equal(0, count set)
        populate()

        context.Articles
        |> Seq.where (fun i -> i.Content = "Foo")
        |> Seq.length
        |> fun count -> Assert.Equal(count, 2)
        clear context set   

    [<Fact; Log>]
    let ``should return all items satisfying condition``() =
        Assert.Equal(0, count set)
        populate()

        fun (i: ArticleEntity) -> i.Content.Length = 3
        |> context.Articles.AllAsync
        |> await     
        |> Assert.True
        clear context set    

    [<Fact; Log>]
    let ``should determine if table is empty``() =
        Assert.Equal(0, count set)
        populate()

        context.Articles.AnyAsync()
        |> await
        |> Assert.True
        clear context set