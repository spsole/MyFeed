namespace myFeed.Tests.Models

open Xunit

open System
open System.Linq

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging

open myFeed.Entities
open myFeed.Entities.Local

open myFeed.Repositories.Abstractions

open myFeed.Tests.Extensions
open myFeed.Tests.Logging

/// Helpers making work with EF contexts easier.
module ModelHelpers =         

    // Saves changes to hard disk.
    let save (context: DbContext) = 
        context.SaveChanges() |> ignore

    // Counts elements in DbSet.
    let count<'D when 'D: not struct> (context: DbContext) =
        context.Set<'D>().CountAsync() |> await

    // Clears given DbSet.
    let clear<'D when 'D: not struct> (context: DbContext) =
        let set = context.Set<'D>()
        set |> set.RemoveRange
        save context |> ignore

    // Migrates database if it's not migrated.
    let migrate (context: DbContext) =
        if (not <| context.Database.GetAppliedMigrations().Any()) then
            context.Database.MigrateAsync() |> awaitTask   

    // Builds context using LoggerFactory.
    let buildLoggableContext() =
        let factory = new LoggerFactory()
        factory.AddProvider(new XUnitLoggerProvider())
        new EntityContext(factory) |> also migrate 

/// Tests for source categories and sources tables 
/// in Entityframework Core database.
module SourcesTableTests = 
    open ModelHelpers

    let populate (context: DbContext) = 
        [ SourceCategoryEntity(Title="Foo", 
            Sources=([| SourceEntity(Uri="foo", Notify=true);
                        SourceEntity(Uri="bar", Notify=false) |]));
          SourceCategoryEntity(Title="Bar", 
            Sources=([| SourceEntity(Uri="foobar", Notify=false) |])) ]
        |> context.Set<SourceCategoryEntity>().AddRange
        save context

    [<Fact>]
    let ``should populate table with items``() =
        buildLoggableContext()
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also populate
        |> also (count<SourceCategoryEntity> >> Should.equal 2)
        |> also clear<SourceCategoryEntity>
        |> dispose
        
    [<Fact>]
    let ``should be able to select items from table``() =
        buildLoggableContext()
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt ->
            ctxt.Set<SourceCategoryEntity>()
                .FirstAsync(fun x -> x.Title = "Foo")
            |@> fun category -> category.Sources
            |> Seq.where (fun x -> x.Uri.Length = 3)
            |> Seq.length
            |> Should.equal 2)
        |> also clear<SourceCategoryEntity>
        |> dispose         

    [<Fact>]
    let ``should be able to select items from multiple tables``() =
        buildLoggableContext()
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also populate 
        |> also (fun ctxt -> 
            ctxt.Set<SourceCategoryEntity>()
                .Include(fun x -> x.Sources)
            |> Seq.toList            
            |> Seq.collect (fun x -> x.Sources)
            |> Seq.length
            |> Should.equal 3)
        |> also clear<SourceCategoryEntity>
        |> dispose        

/// CRUD tests for configuration entities tables 
/// in Entityframework local database.
module ConfigurationTableTests =
    open ModelHelpers

    let populate (context: DbContext) =
        ConfigurationEntity(Key="Foo", Value="Bar")
        |> context.Set<ConfigurationEntity>().Add 
        |> ignore; save context

    [<Fact>]
    let ``should be able to insert and remove items``() =
        buildLoggableContext()
        |> also (count<ConfigurationEntity> >> Should.equal 0)
        |> also populate
        |> also (count<ConfigurationEntity> >> Should.equal 1)
        |> also clear<ConfigurationEntity>
        |> dispose            

    [<Fact>]
    let ``should select item by it's key``() =
        buildLoggableContext()
        |> also (count<ConfigurationEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt -> 
            ctxt.Set<ConfigurationEntity>()
                .FirstAsync(fun i -> i.Key = "Foo")
            |@> fun x -> x.Value
            |> Should.equal "Bar")
        |> also clear<ConfigurationEntity>
        |> dispose

/// CRUD tests for articles table in 
/// EntityFramework Core database table.
module ArticlesTableTests =
    open ModelHelpers

    let populate (context: DbContext) =
        [ ArticleEntity(Content="Foo", FeedTitle="Bar");
          ArticleEntity(Content="Foo", FeedTitle="Foo");
          ArticleEntity(Content="Bar", FeedTitle="Foobar") ]
        |> context.Set<ArticleEntity>().AddRange 
        save context

    [<Fact>]
    let ``should be able to insert items``() =
        buildLoggableContext()
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also populate
        |> also (count<ArticleEntity> >> Should.equal 3)
        |> also clear<ArticleEntity>
        |> dispose

    [<Fact>]
    let ``should find item in database``() =
        buildLoggableContext()
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt -> 
            ctxt.Set<ArticleEntity>()
                .FirstAsync(fun x -> x.Content = "Bar")
            |@> fun found -> found.FeedTitle
            |> Should.equal "Foobar")
        |> also clear<ArticleEntity>
        |> dispose        

    [<Fact>]
    let ``should select nessesary items from database``() =
        buildLoggableContext()
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt -> 
            ctxt.Set<ArticleEntity>()
            |> Seq.where (fun x -> x.Content = "Foo")
            |> Seq.length
            |> Should.equal 2)
        |> also clear<ArticleEntity>
        |> dispose        

    [<Fact>]
    let ``should return all items satisfying condition``() =
        buildLoggableContext()
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt -> 
            ctxt.Set<ArticleEntity>()
                .AllAsync(fun x -> x.Content.Length = 3)
            |@> Should.equal true)
        |> also clear<ArticleEntity>       
        |> dispose

    [<Fact>]
    let ``should determine if table is empty``() =
        buildLoggableContext()
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also populate
        |> also (fun ctxt -> 
            ctxt.Set<ArticleEntity>()
                .AnyAsync()
            |@> Should.equal true)
        |> also clear<ArticleEntity>
        |> dispose
        