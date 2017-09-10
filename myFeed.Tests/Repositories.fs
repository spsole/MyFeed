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
open myFeed.Repositories.Entities.Local
open myFeed.Repositories.Entities

// Repository fixture that encapsulates repos resolution.
type Repository<'R>() =
    let scope =
        ContainerBuilder()
        |> tee (registerLambda (fun _ -> 
            let factory = new LoggerFactory()
            factory.AddProvider <| new XUnitLoggerProvider()
            new EntityContext(factory))) 
        |> tee registerAs<ArticlesRepository, IArticlesRepository>
        |> tee registerAs<SourcesRepository, ISourcesRepository>
        |> tee registerAs<ConfigurationRepository, IConfigurationRepository>
        |> buildScope
    interface IDisposable with member x.Dispose() = scope.Dispose()
    member x.Instance with get() = scope.Resolve<'R>() 

// Tests for articles repository.
module ArticlesRepositoryTests =

    let repository = 
        new Repository<IArticlesRepository>()
        |> fun repo -> repo.Instance

    [<Fact>]
    let ``should return all items enumerable``() =
        repository.GetAllAsync() 
        |> await
        |> Assert.NotNull

    [<Fact>]
    let ``should be able to insert items``() =
        let entity = ArticleEntity(Title="Foo")
        repository.InsertAsync entity |> awaitTask
        let title = 
            repository.GetAllAsync() 
            |> await
            |> tee Assert.NotEmpty
            |> Seq.item 0
            |> fun item -> item.Title
        Assert.Equal("Foo", title)
        repository.RemoveAsync entity |> awaitTask
        repository.GetAllAsync()
        |> await
        |> Assert.Empty

    [<Fact>]
    let ``should return articles with sources joined``() =
        let article = 
            ArticleEntity(
                Title="Foo",                
                Source=SourceEntity(
                    Category=SourceCategoryEntity(
                        Title="Bar")))
        repository.InsertAsync article |> awaitTask
        repository.GetAllAsync() 
        |> await
        |> Seq.item 0
        |> fun item -> 
            Assert.Equal("Foo", item.Title)
            Assert.Equal("Bar", item.Source.Category.Title)   
        use context = new EntityContext()
        context.Articles.RemoveRange context.Articles
        context.SourceCategories.RemoveRange context.SourceCategories     
        context.SaveChanges()                  

// Tests for configuration repository.
module ConfigurationRepositoryTests =

    let repository = 
        new Repository<IConfigurationRepository>()
        |> fun repo -> repo.Instance

    [<Fact>]
    let ``should return value using name``() = 
        repository.SetByNameAsync("Foo", "Bar") |> awaitTask
        repository.SetByNameAsync("Bar", "Foo") |> awaitTask
        Assert.Equal("Bar", await <| repository.GetByNameAsync("Foo"))
        Assert.Equal("Foo", await <| repository.GetByNameAsync("Bar"))

// Tests for sources repository.
module SourcesRepositoryTests =

    let repository = 
        new Repository<ISourcesRepository>()
        |> fun repo -> repo.Instance

    [<Fact>]
    let ``should return all items enumerable``() = 
        repository.GetAllAsync()
        |> await
        |> Assert.NotNull

    [<Fact>]
    let ``should insert items into table and order them``() =
        let entities = 
            [| SourceCategoryEntity(Title="Foo"); 
               SourceCategoryEntity(Title="Bar") |]
        repository.InsertAsync entities |> awaitTask     

        let all = await <| repository.GetAllAsync() 
        let lst = all |> List.ofSeq

        Assert.Equal(1, lst.[0].Order)
        Assert.Equal(2, lst.[1].Order)
        Assert.Equal("Foo", lst.[0].Title)
        Assert.Equal("Bar", lst.[1].Title)
        
        repository.RemoveAsync(all |> Array.ofSeq) |> awaitTask

    [<Fact>]
    let ``should be able to insert and remove entities``() =
        let entry = 
            SourceCategoryEntity(
                Sources=(   
                    [SourceEntity()] 
                    |> collection))
        repository.InsertAsync entry |> awaitTask
        Assert.Equal(1, repository.GetAllAsync() |> await |> Seq.length)
        repository.RemoveAsync entry |> awaitTask
        Assert.Equal(0, repository.GetAllAsync() |> await |> Seq.length)
        
    [<Fact>]    
    let ``should be able to rename category in table``() = 
        let entry = SourceCategoryEntity()
        repository.InsertAsync entry |> awaitTask

        (entry, "Foo")
        |> repository.RenameAsync
        |> awaitTask

        Assert.Equal("Foo", entry.Title)
        repository.RemoveAsync entry |> awaitTask

    [<Fact>]
    let ``should add source entity to category``() = 
        let category = SourceCategoryEntity(Sources=([] |> collection))
        repository.InsertAsync category |> awaitTask

        (category, SourceEntity())
        |> repository.AddSourceAsync
        |> awaitTask

        Assert.Equal(1, category.Sources |> Seq.length)
        repository.RemoveAsync category |> awaitTask

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
        repository.RemoveAsync category |> awaitTask

    [<Fact>]
    let ``should rearrange categories``() = 
        let sequence =
            [| SourceCategoryEntity(Title="Foo");
               SourceCategoryEntity(Title="Bar");
               SourceCategoryEntity(Title="Foobar") |]
        repository.InsertAsync sequence |> awaitTask

        let first = repository.GetAllAsync() |> await |> Seq.item 0
        Assert.Equal(first.Title, "Foo")

        let rearrangedSequence = 
            [ sequence.[1]; 
              sequence.[2]; 
              sequence.[0] ]
        rearrangedSequence 
        |> repository.RearrangeAsync
        |> awaitTask

        let all = repository.GetAllAsync() |> await
        let first = all |> Seq.item 0
        Assert.Equal(first.Title, "Bar")

        repository.RemoveAsync (all |> Array.ofSeq) |> awaitTask