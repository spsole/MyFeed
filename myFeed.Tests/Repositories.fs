namespace myFeed.Tests.Repositories

open Xunit
open Autofac

open System
open System.Linq

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Implementations

open myFeed.Tests.Extensions.EFCoreHelpers
open myFeed.Tests.Extensions.Dep
open myFeed.Tests.Extensions
open myFeed.Tests.Modules

open myFeed.Entities.Local
open myFeed.Entities

/// Test fixture for configuration repository.
type ConfigurationRepositoryTests(repository: ConfigurationRepository) =
    interface IClassFixture<ConfigurationRepository>

    [<Fact>]
    member x.``should return null if key does not exist``() =
        repository.GetByNameAsync("Foo").Result
        |> Should.equal null

    [<Fact>]
    member x.``should set value by key and return it's value``() =
        repository.SetByNameAsync("Foo", "Bar").Wait()
        repository.GetByNameAsync("Foo").Result
        |> Should.equal "Bar" 
        purge<ConfigurationEntity>()

    [<Fact>]
    member x.``should overwrite values using SetByName``() =
        repository.SetByNameAsync("Foo", "Bar").Wait()
        repository.SetByNameAsync("Foo", "Zoo").Wait()    
        repository.GetByNameAsync("Foo").Result
        |> Should.equal "Zoo"   
        purge<ConfigurationEntity>()

/// Test fixture for sources repository logic verifying.
type SourcesRepositoryFixture(repository: SourcesRepository) =
    interface IClassFixture<SourcesRepository>

    [<Fact>]
    member x.``should return all items enumerable``() =
        repository.GetAllAsync().Result
        |> (Seq.length >> Should.equal 0)

    [<Fact>]
    member x.``should insert items into table``() =  
        repository.InsertAsync(SourceCategoryEntity(Title="Foo")).Wait()
        repository.GetAllAsync().Result
        |> (Seq.length >> Should.equal 1)  
        purge<SourceCategoryEntity>()

    [<Fact>]
    member x.``should insert items into table and order them``() =
        repository.InsertAsync(
            [| SourceCategoryEntity(Title="Foo"); 
               SourceCategoryEntity(Title="Bar") |]).Wait()         
        let items = List.ofSeq <| repository.GetAllAsync().Result 

        Should.equal 1 items.[0].Order
        Should.equal 2 items.[1].Order
        Should.equal "Foo" items.[0].Title
        Should.equal "Bar" items.[1].Title
        purge<SourceCategoryEntity>()

    [<Fact>]
    member x.``should be able to insert nested entities also``() =
        repository.InsertAsync(
            SourceCategoryEntity(Title="Foo", 
                Sources=([| SourceEntity(Uri="http://foo.bar") |]))).Wait()        
        let category = Seq.item 0 <| repository.GetAllAsync().Result
        let source = Seq.item 0 <| category.Sources

        Should.equal "Foo" category.Title
        Should.equal 1 category.Sources.Count
        Should.equal "http://foo.bar" source.Uri
        Should.equal category source.Category
        purge<SourceCategoryEntity>()
        
    [<Fact>]    
    member x.``should be able to rename category in table``() = 
        let category = SourceCategoryEntity(Title="Bar")
        repository.InsertAsync(category).Wait()
        repository.RenameAsync(category, "Foo").Wait()       
        let items = List.ofSeq <| repository.GetAllAsync().Result

        Should.equal "Foo" items.[0].Title
        purge<SourceCategoryEntity>()

    [<Fact>]
    member x.``should add source entity to category``() = 
        let category = SourceCategoryEntity()
        repository.InsertAsync(category).Wait()
        repository.AddSourceAsync(category, SourceEntity()).Wait()
        let category = Seq.item 0 <| repository.GetAllAsync().Result

        category.Sources |> (Seq.length >> Should.equal 1)
        purge<SourceCategoryEntity>()
        
    [<Fact>]
    member x.``should remove source entity from table``() =
        let source = SourceEntity()
        let category = SourceCategoryEntity(Sources=collection([ source ]))
        repository.InsertAsync(category).Wait()
        repository.RemoveSourceAsync(category, source).Wait()
        let category = Seq.item 0 <| repository.GetAllAsync().Result
        
        category.Sources |> (Seq.length >> Should.equal 0)
        purge<SourceCategoryEntity>()
        
    [<Fact>]
    member x.``should rearrange categories``() = 
        let categories =
            [| SourceCategoryEntity(Title="Foo");
               SourceCategoryEntity(Title="Bar");
               SourceCategoryEntity(Title="Foobar") |]
        repository.InsertAsync(categories).Wait()
        repository.RearrangeAsync(
            [ categories.[1]; 
              categories.[2]; 
              categories.[0] ]).Wait()
        
        let items = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal "Bar" items.[0].Title
        Should.equal "Foobar" items.[1].Title
        Should.equal "Foo" items.[2].Title
        purge<SourceCategoryEntity>()

/// Test fixture verifying articles repository logic.
type ArticlesRepositoryFixture(repository: ArticlesRepository) =
    interface IClassFixture<ArticlesRepository>

    [<Fact>]
    member x.``should return all items enumerable``() =
        repository.GetAllAsync().Result
        |> (Seq.length >> Should.equal 0)

    [<Fact>]
    member x.``should insert articles into articles table``() =
        let category = SourceCategoryEntity(Sources=[| SourceEntity(Uri="http://foo.bar") |])
        SourcesRepository().InsertAsync(category).Wait()
        repository.InsertAsync(source=(Seq.item 0 category.Sources), entities=
            [| ArticleEntity(Title="Foo") |]).Wait()

        let items = List.ofSeq <| repository.GetAllAsync().Result
        Should.equal 1 (items.Count())
        Should.equal "Foo" items.[0].Title
        Should.equal "http://foo.bar" items.[0].Source.Uri
        purge<SourceCategoryEntity>()
        purge<ArticleEntity>()
        
    [<Fact>]
    member x.``should remove article by instance``() =
        new EntityContext()
        |> also (populate [| ArticleEntity(Title="Foo") |])
        |> dispose       

        let article = Seq.item 0 <| repository.GetAllAsync().Result
        let response = repository.RemoveAsync(article).Wait()
        repository.GetAllAsync().Result
        |> (Seq.length >> Should.equal 0)
        purge<ArticleEntity>()

    [<Fact>]
    member x.``should find article by its global unique identifier``() =
        new EntityContext()
        |> also (populate [| ArticleEntity(Title="Foo") |])
        |> dispose

        let article = Seq.item 0 <| repository.GetAllAsync().Result
        let response = repository.GetByIdAsync(article.Id).Result
        Should.equal "Foo" response.Title
        purge<ArticleEntity>()

    [<Fact>]
    member x.``should return null if article with id does not exist``() =   
        repository.GetByIdAsync(Guid()).Result 
        |> Should.equal null 

    [<Fact>]
    member x.``should remove unreferenced articles from the database``() =
        new EntityContext()
        |> also (populate 
            [ SourceEntity(Articles=
                collection [| ArticleEntity(Title="Bar");
                    ArticleEntity(Title="Foo", Fave=true);
                    ArticleEntity(Title="Zoo") |]) ])
        |> also save
        |> also clear<SourceEntity>
        |> dispose

        repository.GetAllAsync().Result |> (Seq.length >> Should.equal 3)
        repository.RemoveUnreferencedArticles().Wait() 
        repository.GetAllAsync().Result |> (Seq.length >> Should.equal 1)

        purge<ArticleEntity>()
        purge<SourceEntity>()