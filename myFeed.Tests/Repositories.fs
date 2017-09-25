namespace myFeed.Tests.Repositories

open Xunit
open Autofac

open System
open System.Linq

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.Dep
open myFeed.Tests.Logging
open myFeed.Tests.Modules

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Implementations

open myFeed.Entities.Local
open myFeed.Entities

[<AutoOpen>]
module RepositoryHelpers =

    let clear<'a when 'a: not struct> (context: DbContext) =
        context.Set<'a>() |> context.Set<'a>().RemoveRange
        context.SaveChangesAsync() |> awaitTask

    let clearContext() =
        new EntityContext()
        |> also clear<ArticleEntity>
        |> also clear<SourceCategoryEntity>
        |> also clear<ConfigurationEntity>
        |> dispose

    let buildRepository<'a>() =
        ContainerBuilder() 
        |> also registerAs<SourcesRepository, ISourcesRepository>
        |> also registerAs<ArticlesRepository, IArticlesRepository>
        |> also registerAs<ConfigurationRepository, IConfigurationRepository>
        |> buildScope 
        |> resolveOnce<'a>

/// Tests for configuration EFCore 
/// repository open API methods.
module ConfigurationRepositoryTests =
    let repository = buildRepository<IConfigurationRepository>()

    [<Fact>]
    let ``should set and return values using names``() = 
        repository.SetByNameAsync("Foo", "Bar")   |> awaitTask
        repository.SetByNameAsync("Bar", "Foo")   |> awaitTask
        repository.GetByNameAsync("Foo") |> await |> Should.equal "Bar"
        repository.GetByNameAsync("Bar") |> await |> Should.equal "Foo"
        clearContext()

// Tests for sources repository.
module SourcesRepositoryTests =
    let repository = buildRepository<ISourcesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() = 
        repository.GetAllAsync() 
        |> await
        |> Seq.length 
        |> Should.equal 0

    [<Fact>]
    let ``should insert items into table and order them``() =
        let entities = 
            [| SourceCategoryEntity(Title="Foo"); 
               SourceCategoryEntity(Title="Bar") |]
        repository.InsertAsync entities |> awaitTask     
        let categories = 
            repository.GetAllAsync() 
            |> await
            |> List.ofSeq
        Should.equal 1 categories.[0].Order
        Should.equal 2 categories.[1].Order
        Should.equal "Foo" categories.[0].Title
        Should.equal "Bar" categories.[1].Title
        clearContext()

    [<Fact>]
    let ``should be able to insert entities``() =
        let category = 
            SourceCategoryEntity(
                Title="Foo", Sources=
                    ([| SourceEntity(Uri="http://foo.bar") |]))
        repository.InsertAsync category |> awaitTask
        repository.GetAllAsync() 
        |> await
        |> Seq.item 0
        |> fun category -> category.Sources
        |> Seq.item 0
        |> fun source -> source.Uri
        |> Should.equal "http://foo.bar"
        clearContext()
        
    [<Fact>]    
    let ``should be able to rename category in table``() = 
        let entry = SourceCategoryEntity()
        repository.InsertAsync entry |> awaitTask
        repository.RenameAsync(entry, "Foo") |> awaitTask
        Should.equal "Foo" entry.Title
        clearContext()

    [<Fact>]
    let ``should add source entity to category``() = 
        let category = SourceCategoryEntity()
        repository.InsertAsync category |> awaitTask

        repository.AddSourceAsync(category, SourceEntity()) |> awaitTask
        category.Sources
        |> Seq.length
        |> Should.equal 1
        clearContext()

    [<Fact>]
    let ``should remove source entity from table``() =
        let category = SourceCategoryEntity()
        let source = SourceEntity()
        category.Sources.Add <| source

        repository.InsertAsync category |> awaitTask
        category.Sources
        |> Seq.length
        |> Should.equal 1

        repository.RemoveSourceAsync(category, source) |> awaitTask
        category.Sources
        |> Seq.length
        |> Should.equal 0
        clearContext()

    [<Fact>]
    let ``should rearrange categories``() = 
        let sequence =
            [| SourceCategoryEntity(Title="Foo");
               SourceCategoryEntity(Title="Bar");
               SourceCategoryEntity(Title="Foobar") |]
        repository.InsertAsync sequence |> awaitTask
        repository.GetAllAsync() 
        |> await
        |> Seq.item 0
        |> fun entity -> entity.Title
        |> Should.equal "Foo"

        [ sequence.[1]; 
          sequence.[2]; 
          sequence.[0] ]
        |> repository.RearrangeAsync
        |> awaitTask
        
        repository.GetAllAsync() 
        |> await
        |> Seq.item 0
        |> fun entity -> entity.Title
        |> Should.equal "Bar"
        clearContext()

// Tests for articles repository.
module ArticlesRepositoryTests =
    let repository = buildRepository<IArticlesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() =
        repository.GetAllAsync() 
        |> await
        |> Seq.length 
        |> Should.equal 0

    [<Fact>]
    let ``should be able to insert items``() = 
        let source = SourceEntity()
        use context = new EntityContext()
        SourceCategoryEntity(Sources=([| source |]))
        |> context.Set<SourceCategoryEntity>().Add |> ignore
        context.SaveChanges() |> ignore

        repository.InsertAsync (source=source, entities=
            [| ArticleEntity(Title="Foo") |]) |> awaitTask     
        repository.GetAllAsync() 
        |> await
        |> Seq.item 0
        |> fun entity -> entity.Title
        |> Should.equal "Foo"
        clearContext()

    [<Fact>]
    let ``should return article by id``() =
        use context = new EntityContext()
        ArticleEntity(Title="Foo")
        |> context.Set<ArticleEntity>().Add |> ignore
        context.SaveChanges() |> ignore

        repository.GetAllAsync() 
        |> await
        |> Seq.item 0        
        |> fun first -> first.Id
        |> repository.GetByIdAsync
        |> await
        |> fun article -> article.Title
        |> Should.equal "Foo"
        clearContext() 

    [<Fact>]
    let ``should return null if article with id does not exist``() =    
        Guid()
        |> repository.GetByIdAsync
        |> await
        |> Should.beNull
        