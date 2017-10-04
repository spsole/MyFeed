namespace myFeed.Tests.Extensions

open System
open System.Linq
open System.Reflection
open System.Threading.Tasks
open System.Collections.Generic

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Storage

open Xunit
open Autofac
open NSubstitute
open myFeed.Entities

[<AutoOpen>]
module Tools =

    /// Converts sequence to collection.    
    let collection (sequence: seq<'a>) = sequence.ToList()

    /// Disposes IDisposable in func style. 
    let dispose (disposable: IDisposable) = disposable.Dispose()

    /// Attaches handler to IEvent.
    let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) = event.Add handler
    
    /// C#-like await operator.
    let await (task: Task<'a>) = task.Result

    /// C#-like task await operator.
    let awaitTask (task: Task) = task.Wait()

    /// "Tee" functions.
    let inline also f x = f x; x  

    /// First for structs.
    let fst struct (a, b) = a 

    /// Second for structs.
    let snd struct (a, b) = b

/// Dependency injection module.
module Dep =

    /// Registers type as abstraction.
    let registerAs<'a, 'i> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().As<'i>() |> ignore

    /// Registers type as self.
    let registerAsSelf<'a> (builder: ContainerBuilder) =
        builder.RegisterType<'a>().AsSelf() |> ignore  

    /// Registers instance as abstraction.
    let registerInstanceAs<'i> (instance: obj) (builder: ContainerBuilder) =
        builder.RegisterInstance(instance).As<'i>() |> ignore  

    /// Registers lambda as abstraction.
    let registerLambda<'a> (func: IComponentContext -> 'a) (builder: ContainerBuilder) =
        builder.Register(func).AsSelf() |> ignore

    /// Registers module into container builder.
    let registerModule<'m when 'm: (new: unit -> 'm) and 'm :> Core.IModule> (builder: ContainerBuilder) =
        builder.RegisterModule<'m>() |> ignore     

    /// Registers mock instance info builder.
    let registerMock<'a when 'a: not struct> (builder: ContainerBuilder) =
        let mockInstance = NSubstitute.Substitute.For<'a>()
        builder |> registerInstanceAs<'a> mockInstance |> ignore

    /// Resolves stuff from scope.
    let resolve<'i> (scope: ILifetimeScope) = 
        scope.Resolve<'i>() 

    /// Resolves something from scope and disposes it.
    let resolveOnce<'i> (scope: ILifetimeScope) =
        let instance = resolve<'i> scope
        dispose scope; instance    

    /// Builds scope from a builder.        
    let buildScope (builder: ContainerBuilder) = 
        builder.Build()   

/// FSharpy fluent assertions.
module Should =

    /// Ensures a is null.
    let beNull<'a> (a: 'a) = 
        Assert.Null(a)

    /// Ensures a is not null.
    let notBeNull<'a> (a: 'a) = 
        Assert.NotNull(a)

    /// Ensures a equals b.
    let equal<'a> (a: 'a) (b: 'a) = 
        Assert.Equal(a, b)
    
    /// Ensures a not equals b.
    let notEqual<'a> (a: 'a) (b: 'a) = 
        Assert.NotEqual(a, b)

    /// Ensures string contains substring.
    let contain (a: string) (b: string) =
        Assert.Contains(a, b)

    /// Ensures instance can be resolved from scope.
    let resolve<'a> = Dep.resolve<'a> >> notBeNull

/// Helpers making work with EF contexts easier.
module EFCoreHelpers =         

    // Saves changes to hard disk.
    let save (context: DbContext) = 
        context.SaveChanges() |> ignore

    // Counts elements in DbSet.
    let count<'a when 'a: not struct> (context: DbContext) =
        context.Set<'a>().CountAsync() |> await

    // Clears given DbSet.
    let clear<'a when 'a: not struct> (context: DbContext) =
        let set = context.Set<'a>()
        set |> set.RemoveRange
        save context |> ignore

    /// Populates context with data.
    let populate<'a when 'a: not struct> (values: seq<'a>) (context: DbContext) =
        values |> context.Set<'a>().AddRange
        save context |> ignore

    // Migrates database if it's not migrated.
    let migrate (context: DbContext) =
        if (not <| context.Database.GetAppliedMigrations().Any()) then
            context.Database.MigrateAsync() |> awaitTask  

    /// Purges physical data using type args.
    let purge<'a when 'a: not struct> () =
        use context = new EntityContext()
        clear<'a> context

    // Builds context using LoggerFactory.
    let buildLoggableContext() =
        let logger = {
            new ILogger with 
                member x.IsEnabled lvl = true
                member x.BeginScope state = Substitute.For<IDisposable>()
                member x.Log (level, eventId, state: 'a, ex, formatter) =
                    if (not <| Object.Equals(state, null)) then 
                        match level with
                        | LogLevel.Debug -> ()
                        | LogLevel.Information -> 
                            match box state with 
                            | :? DbCommandLogData as cmd ->
                                cmd.CommandText.Replace("\r\n", " ") 
                                |> sprintf "[myFeed.Tests] %s" 
                                |> (Console.WriteLine >> also System.Diagnostics.Debug.WriteLine)
                            | _ -> ()
                        | _ -> () }
        let provider = { 
            new ILoggerProvider with
                member x.Dispose () = ()
                member x.CreateLogger name = logger }
        let factory = new LoggerFactory()
        factory.AddProvider provider        
        new EntityContext(factory) 
        |> also migrate 
