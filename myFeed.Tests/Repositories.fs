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
type Repository<'E, 'R when 'R :> IAbstractRepository<'E>>() =
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

[<AutoOpen>]
module RepositoryHelpers =
    
    /// Clears passed repository.
    let clear<'a> (repo: IAbstractRepository<'a>) =
        repo.GetAllAsync() 
        |> await
        |> Seq.iter (repo.RemoveAsync >> awaitTask >> ignore)
        
    /// Inserts item into passed repository.    
    let insert<'a> (repo: IAbstractRepository<'a>) entity =
        entity |> repo.InsertAsync |> awaitTask 

    /// Removes item from repository.
    let remove<'a> (repo: IAbstractRepository<'a>) entity =
        entity |> repo.RemoveAsync |> awaitTask   
        
    /// Counts entries in repo by using GetAll method.    
    let count<'a> (repo: IAbstractRepository<'a>) =
        repo.GetAllAsync() |> await |> Seq.length  

    /// Builds scope and resolves repository.
    let buildRepo<'e, 'r when 'r :> IAbstractRepository<'e>> () =
        let fixture = new Repository<'e, 'r>()
        fixture.Instance 

// Tests for articles repository.
module ArticlesRepositoryTests =

    let repository = buildRepo<ArticleEntity, IArticlesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() =
        Assert.Equal(0, count repository)   

    [<Fact>]
    let ``should be able to insert and remove items``() =
        let entity = ArticleEntity()
        entity |> insert repository
        Assert.Equal(1, count repository)

        entity |> remove repository
        Assert.Equal(0, count repository)
        clear repository 

    [<Fact>]
    let ``should return articles with sources joined``() =
        let source = SourceEntity()
        source.Articles.Add <| ArticleEntity(Title="Bar")
        let category = 
            SourceCategoryEntity(
                Title="Foo", 
                Sources=([ source ] |> collection))

        let sourcesRepo = buildRepo<SourceCategoryEntity, ISourcesRepository>()
        sourcesRepo.InsertAsync category |> awaitTask
        let all = repository.GetAllAsync() |> await

        Assert.Equal("Bar", all.First().Title)
        clear sourcesRepo
        clear repository

// Tests for configuration repository.
module ConfigurationRepositoryTests =

    let repository = buildRepo<ConfigurationEntity, IConfigurationRepository>()

    [<Fact>]
    let ``should return all items``() = 
        Assert.Equal(0, count repository)

    [<Fact>]
    let ``should be able to insert and remove items``() =
        let entity = ConfigurationEntity(Key="Foo", Value="Bar")
        entity |> insert repository
        Assert.Equal(1, count repository)
        
        entity |> remove repository
        Assert.Equal(0, count repository)
        clear repository 

    [<Fact>]
    let ``should return value using name``() = 
        [ ConfigurationEntity(Key="Foo", Value="0");
          ConfigurationEntity(Key="Bar", Value="1") ]
        |> Seq.iter (insert repository)  
        Assert.Equal(2, count repository)

        repository.GetByNameAsync("Bar")
        |> await
        |> fun value -> Assert.Equal("1", value)
        clear repository

    [<Fact>]
    let ``should set pair value using pair name``() =    
        repository.SetByNameAsync("Foo", "Bar") |> awaitTask
        let value = await <| repository.GetByNameAsync("Foo")
        Assert.Equal("Bar", value)
        
        repository.SetByNameAsync("Foo", "NotBar") |> awaitTask
        let value = await <| repository.GetByNameAsync("Foo")

        Assert.Equal("NotBar", value)
        clear repository

// Tests for sources repository.
module SourcesRepositoryTests =

    let repository = buildRepo<SourceCategoryEntity, ISourcesRepository>()

    [<Fact>]
    let ``should return all items enumerable``() = 
        Assert.Equal(0, count repository)

    [<Fact>]
    let ``should insert items into table``() =
        let categories = 
            [ SourceCategoryEntity(); 
              SourceCategoryEntity() ]
        categories          
        |> Seq.iter (repository.InsertAsync >> awaitTask >> ignore)
        Assert.Equal(2, count repository)

        Assert.Equal(1, categories.[0].Order)
        Assert.Equal(2, categories.[1].Order)
        clear repository

    [<Fact>]
    let ``should be able to remove entities``() =
        let entry = 
            SourceCategoryEntity(
                Sources=(   
                    [SourceEntity()] 
                    |> collection))
        entry |> insert repository
        Assert.Equal(1, count repository)
        
        entry |> remove repository
        Assert.Equal(0, count repository)
        clear repository
        
    [<Fact>]    
    let ``should be able to rename category in table``() = 
        let entry = SourceCategoryEntity()
        entry |> insert repository

        (entry, "Foo")
        |> repository.RenameCategoryAsync
        |> awaitTask

        Assert.Equal("Foo", entry.Title)
        clear repository

    [<Fact>]
    let ``should add entity to table``() = 
        let category = SourceCategoryEntity(Sources=([] |> collection))
        category |> insert repository

        (category, SourceEntity())
        |> repository.AddSourceAsync
        |> awaitTask

        Assert.Equal(1, category.Sources |> Seq.length)
        clear repository

    [<Fact>]
    let ``should remove source entity from table``() =
        let category = SourceCategoryEntity()
        let source = SourceEntity()
        category.Sources.Add <| source
        category |> insert repository

        Assert.Equal(1, category.Sources |> Seq.length)
        (category, source)
        |> repository.RemoveSourceAsync
        |> awaitTask

        Assert.Equal(0, category.Sources |> Seq.length)
        clear repository

    [<Fact>]
    let ``should return all categories ordered``() = 
        let sequence =
            [ SourceCategoryEntity(Title="Foo");
              SourceCategoryEntity(Title="Bar");
              SourceCategoryEntity(Title="Foobar") ]
        sequence 
        |> Seq.iter (insert repository)

        let assign (entity: SourceCategoryEntity) order =
            entity.Order <- order
            entity 
            |> repository.UpdateAsync 
            |> awaitTask

        assign sequence.[0] 2
        assign sequence.[1] 1

        repository.GetAllAsync()
        |> await
        |> Seq.length
        |> fun len -> Assert.Equal(3, len)

        repository.GetAllOrderedAsync()
        |> await
        |> Seq.item 0
        |> fun item -> Assert.Equal(item.Title, "Bar")
        clear repository

    [<Fact>]
    let ``should rearrange categories``() = 
        let sequence =
            [ SourceCategoryEntity(Title="Foo");
              SourceCategoryEntity(Title="Bar");
              SourceCategoryEntity(Title="Foobar") ]
        sequence 
        |> Seq.iter (insert repository)

        repository.GetAllOrderedAsync()
        |> await
        |> Seq.item 0
        |> fun i -> Assert.Equal(i.Title, "Foo")

        let rearrangedSequence = 
            [ sequence.[1]; 
              sequence.[2]; 
              sequence.[0] ]
        rearrangedSequence 
        |> repository.RearrangeCategoriesAsync
        |> awaitTask

        repository.GetAllOrderedAsync()
        |> await
        |> Seq.item 0
        |> fun i -> Assert.Equal(i.Title, "Bar")
        clear repository

    [<Fact>]
    let ``should save entities to their tables``() =
        let category = SourceCategoryEntity(Title="Foo")
        let source = SourceEntity(Uri="http://foo.bar")

        category
        |> repository.InsertAsync
        |> awaitTask
        (category, source)
        |> repository.AddSourceAsync
        |> awaitTask

        let categories = await <| repository.GetAllOrderedAsync()
        Assert.Equal(1, categories |> Seq.length)
        
        let category = categories |> Seq.item 0
        Assert.Equal("Foo", category.Title)
        Assert.NotNull(category.Sources)

        let entry = category.Sources |> Seq.item 0
        Assert.Equal("http://foo.bar", entry.Uri)
        clear repository

    [<Fact>]
    let ``should save entities and be able to read them AFTER reload``() =    
        let category = SourceCategoryEntity(Title="Foo")
        let source = SourceEntity(Uri="http://foo.bar")

        category
        |> repository.InsertAsync
        |> awaitTask
        (category, source)
        |> repository.AddSourceAsync
        |> awaitTask

        let newRepo = buildRepo<SourceCategoryEntity, ISourcesRepository>()
        let categories = await <| newRepo.GetAllOrderedAsync()
        let category = categories |> Seq.item 0
        let source = 
            category
            |> fun x -> x.Sources
            |> Seq.item 0

        Assert.Equal(1, categories |> Seq.length)
        Assert.Equal("Foo", category.Title)
        Assert.Equal("http://foo.bar", source.Uri)
        clear repository