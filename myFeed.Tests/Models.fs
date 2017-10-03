namespace myFeed.Tests.Models

open Xunit
open System
open System.Linq

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging

open myFeed.Entities
open myFeed.Entities.Local

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.EFCoreHelpers

/// Tests fixture for sources entities.
type SourcesTableFixture() = 
    let loggableContext = buildLoggableContext()
    let sampleDataset = 
        [ SourceCategoryEntity(Title="Foo", 
            Sources=([| SourceEntity(Uri="foo", Notify=true);
                        SourceEntity(Uri="bar", Notify=false) |]));
          SourceCategoryEntity(Title="Bar", 
            Sources=([| SourceEntity(Uri="foobar", Notify=false) |])) ]

    [<Fact>] 
    member x.``should populate table with items``() =
        loggableContext
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (count<SourceCategoryEntity> >> Should.equal 2)
        |> clear<SourceCategoryEntity>
        
    [<Fact>]
    member x.``should be able to select items from table``() =
        loggableContext
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context ->
            context.Set<SourceCategoryEntity>()
                .FirstAsync(fun x -> x.Title = "Foo")
            |> await
            |> fun category -> category.Sources
            |> Seq.where (fun x -> x.Uri.Length = 3)
            |> Seq.length
            |> Should.equal 2)
        |> clear<SourceCategoryEntity>

    [<Fact>]
    member x.``should be able to select items from multiple tables``() =
        loggableContext
        |> also (count<SourceCategoryEntity> >> Should.equal 0)
        |> also (populate sampleDataset) 
        |> also (fun context -> 
            context.Set<SourceCategoryEntity>()
                .Include(fun x -> x.Sources)
            |> Seq.toList            
            |> Seq.collect (fun x -> x.Sources)
            |> Seq.length
            |> Should.equal 3)
        |> clear<SourceCategoryEntity>

/// Tests fixture for configuration entities.
type ConfigurationTableFixture() =
    let loggableContext = buildLoggableContext()
    let sampleDataset = [ ConfigurationEntity(Key="Foo", Value="Bar") ]

    [<Fact>]
    member x.``should be able to insert and remove items``() =
        loggableContext
        |> also (count<ConfigurationEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (count<ConfigurationEntity> >> Should.equal 1)
        |> clear<ConfigurationEntity>

    [<Fact>]
    member x.``should select item by it's key``() =
        loggableContext
        |> also (count<ConfigurationEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context -> 
            context.Set<ConfigurationEntity>()
                .FirstAsync(fun i -> i.Key = "Foo")
            |> await
            |> fun entity -> entity.Value
            |> Should.equal "Bar")
        |> clear<ConfigurationEntity>

/// Tests fixture for article entities.
type ArticlesTableFixture() =
    let loggableContext = buildLoggableContext()
    let sampleDataset = 
        [ ArticleEntity(Content="Foo", FeedTitle="Bar");
          ArticleEntity(Content="Foo", FeedTitle="Foo");
          ArticleEntity(Content="Bar", FeedTitle="Foobar") ]

    [<Fact>]
    member x.``should be able to insert items``() =
        loggableContext
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (count<ArticleEntity> >> Should.equal 3)
        |> clear<ArticleEntity>

    [<Fact>]
    member x.``should find item in database``() =
        loggableContext
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context -> 
            context.Set<ArticleEntity>()
                .FirstAsync(fun x -> x.Content = "Bar")
            |> await
            |> fun found -> found.FeedTitle
            |> Should.equal "Foobar")
        |> clear<ArticleEntity>

    [<Fact>]
    member x.``should select nessesary items from database``() =
        loggableContext
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context -> 
            context.Set<ArticleEntity>()
            |> Seq.where (fun x -> x.Content = "Foo")
            |> Seq.length
            |> Should.equal 2)
        |> clear<ArticleEntity>

    [<Fact>]
    member x.``should return all items satisfying condition``() =
        loggableContext
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context -> 
            context.Set<ArticleEntity>()
                .AllAsync(fun x -> x.Content.Length = 3)
            |> await
            |> Should.equal true)
        |> clear<ArticleEntity>       

    [<Fact>]
    member x.``should determine if table is empty``() =
        loggableContext
        |> also (count<ArticleEntity> >> Should.equal 0)
        |> also (populate sampleDataset)
        |> also (fun context -> 
            context.Set<ArticleEntity>().AnyAsync()
            |> await
            |> Should.equal true)
        |> clear<ArticleEntity>
        