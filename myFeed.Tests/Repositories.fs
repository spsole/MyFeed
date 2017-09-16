namespace myFeed.Tests.Repositories

open System
open System.Linq

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore

open Xunit
open Autofac

open myFeed.Tests.Extensions
open myFeed.Tests.Extensions.DependencyInjection
open myFeed.Tests.Logging
open myFeed.Tests.Modules

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Implementations

open myFeed.Entities.Local
open myFeed.Entities

[<AutoOpen>]
module RepositoryHelpers =

    let clearSet<'a when 'a: not struct> (context: DbContext) =
        context.Set<'a>() |> context.Set<'a>().RemoveRange
        context.SaveChangesAsync() |> awaitTask

    let clearContext () =
        new EntityContext()
        |> tee clearSet<ArticleEntity>
        |> tee clearSet<SourceCategoryEntity>
        |> tee clearSet<ConfigurationEntity>
        |> dispose

    let buildRepository<'a> () =
        use scope = 
            ContainerBuilder() 
            |> tee registerAs<SourcesRepository, ISourcesRepository>
            |> tee registerAs<ArticlesRepository, IArticlesRepository>
            |> tee registerAs<ConfigurationRepository, IConfigurationRepository>
            |> buildScope 
        let repository = resolve<'a> scope
        dispose scope
        repository          

// Tests for configuration repository.
module ConfigurationRepositoryTests =
    let repository = buildRepository<IConfigurationRepository>()

    [<Fact>]
    let ``should return value using name``() = 
        repository.SetByNameAsync("Foo", "Bar") |> awaitTask
        repository.SetByNameAsync("Bar", "Foo") |> awaitTask
        Assert.Equal("Bar", await <| repository.GetByNameAsync("Foo"))
        Assert.Equal("Foo", await <| repository.GetByNameAsync("Bar"))

// Tests for sources repository.
module SourcesRepositoryTests =
    let repository = buildRepository<ISourcesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() = 
        repository.GetAllAsync() |@> Assert.NotNull

    [<Fact>]
    let ``should insert items into table and order them``() =
        let entities = 
            [| SourceCategoryEntity(Title="Foo"); 
               SourceCategoryEntity(Title="Bar") |]
        repository.InsertAsync entities |> awaitTask     
        let categories = repository.GetAllAsync() |@> List.ofSeq

        Assert.Equal(1, categories.[0].Order)
        Assert.Equal(2, categories.[1].Order)
        Assert.Equal("Foo", categories.[0].Title)
        Assert.Equal("Bar", categories.[1].Title)
        clearContext()

    [<Fact>]
    let ``should be able to insert entities``() =
        let category = 
            SourceCategoryEntity(
                Title="Foo", Sources=
                    ([| SourceEntity(Uri="http://foo.bar") |]))
        repository.InsertAsync category |> awaitTask
        let source = 
            repository.GetAllAsync() 
            |@> Seq.item 0
            |> fun x -> x.Sources
            |> Seq.item 0
        Assert.Equal("http://foo.bar", source.Uri)                
        clearContext()
        
    [<Fact>]    
    let ``should be able to rename category in table``() = 
        let entry = SourceCategoryEntity()
        repository.InsertAsync entry |> awaitTask
        (entry, "Foo")
        |> repository.RenameAsync
        |> awaitTask
        Assert.Equal("Foo", entry.Title)
        clearContext()

    [<Fact>]
    let ``should add source entity to category``() = 
        let category = SourceCategoryEntity()
        repository.InsertAsync category |> awaitTask
        (category, SourceEntity())
        |> repository.AddSourceAsync
        |> awaitTask
        
        Assert.Equal(1, category.Sources |> Seq.length)
        clearContext()

    [<Fact>]
    let ``should remove source entity from table``() =
        let category = SourceCategoryEntity()
        let source = SourceEntity()
        category.Sources.Add <| source
        repository.InsertAsync category |> awaitTask

        Assert.Equal(1, category.Sources |> Seq.length)
        (category, source)
        |> repository.RemoveSourceAsync
        |> awaitTask

        Assert.Equal(0, category.Sources |> Seq.length)
        clearContext()

    [<Fact>]
    let ``should rearrange categories``() = 
        let sequence =
            [| SourceCategoryEntity(Title="Foo");
               SourceCategoryEntity(Title="Bar");
               SourceCategoryEntity(Title="Foobar") |]
        repository.InsertAsync sequence |> awaitTask

        let first = repository.GetAllAsync() |@> Seq.item 0
        Assert.Equal(first.Title, "Foo")

        let rearrangedSequence = 
            [ sequence.[1]; 
              sequence.[2]; 
              sequence.[0] ]
        rearrangedSequence 
        |> repository.RearrangeAsync
        |> awaitTask

        let first = repository.GetAllAsync() |@> Seq.item 0
        Assert.Equal(first.Title, "Bar")
        clearContext()

// Tests for articles repository.
module ArticlesRepositoryTests =
    let repository = buildRepository<IArticlesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() =
        repository.GetAllAsync() |@> Assert.NotNull

    [<Fact>]
    let ``should be able to insert items``() = 
        let source = SourceEntity()

        buildRepository<ISourcesRepository>().InsertAsync (
            SourceCategoryEntity(Sources=([| source |]))) |> awaitTask
        repository.InsertAsync (source=source, entities=
            [| ArticleEntity(Title="Foo") |]) |> awaitTask     

        let article = repository.GetAllAsync() |@> Seq.item 0
        Assert.Equal("Foo", article.Title)
        clearContext()