[<AutoOpen>]
module myFeed.Tests.Extensions.Default

open Xunit
open Xunit.Sdk
open LiteDB
open DryIoc
open System
open System.IO
open System.Linq
open System.Reflection
open System.Windows.Input
open System.Threading.Tasks
open myFeed.Platform
open NSubstitute
open System.Reactive

/// Converts sequence to list.
let toList (sequence: seq<'a>) = sequence.ToList()

/// Disposes disposable in functional style.
let dispose (disposable: IDisposable) = disposable.Dispose()

/// C#-like event subscription operator.
let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) = event.Add handler

/// Applies function to argument and continues.
let inline also defun arguments = defun arguments; arguments

/// Extracts first element from tuple.
let fst (first, _) = first 

/// Extracts second element from tuple.
let snd (_, second) = second   

/// Produces instance of an object with dependencies supplied.
let produce<'T when 'T : not struct> (injectables: obj seq) =
    let substituteIfNone (parameterInfo: ParameterInfo) =
        let parameterType = parameterInfo.ParameterType
        injectables 
        |> Seq.tryFind (fun x -> 
            let objectType = x.GetType() 
            objectType = parameterType ||
            objectType.GetInterfaces()
            |> Seq.contains parameterType)
        |> function
        | Some some -> some
        | None -> Substitute.For([| parameterType |], null)

    let objectType = typeof<'T>
    let ctor = Seq.item 0 <| objectType.GetConstructors()
    ctor.GetParameters()
    |> Seq.map substituteIfNone
    |> Seq.cast<obj>
    |> Array.ofSeq
    |> ctor.Invoke :?> 'T   

// ICommand abstraction.
type ICommand with 

    // Wraps command state into a Task.
    member command.Invoke() =
        let source = TaskCompletionSource<bool>()
        command.CanExecuteChanged += fun _ -> 
            if command.CanExecute() then 
                source.SetResult(true) |> ignore
        command.Execute(Unit.Default)       
        source.Task      

/// Assertions module.
type Should = 
    
    /// Asserts that a is nothing.
    static member beNull<'a> (a: 'a) = Assert.Null(a)
    
    /// Asserts that a is not nothing.
    static member notBeNull<'a> (a: 'a) = Assert.NotNull(a)
    
    /// Asserts that a equals b.
    static member equal<'a> (a: 'a) (b: 'a) = Assert.Equal(a, b)
    
    /// Asserts that a does not equal b.
    static member notEqual<'a> (a: 'a) (b: 'a) = Assert.NotEqual(a, b)
    
    /// Asserts that a string contains b substring.
    static member contain (a: string) (b: string) = Assert.Contains(a, b)
        
    /// Asserts that particular method throws an exception of a given type.    
    static member throw<'e when 'e :> exn> act = Assert.Throws<'e>(Action(act))   
    
    /// Ensures that sequence is not empty.    
    static member notBeEmpty<'a, 'b when 'a :> seq<'b>> (ls: 'a) = Assert.NotEmpty   
    
    /// Ensures that particular component has been already registered.
    static member resolve<'a> (scope: IResolver) = scope.Resolve<'a>() |> Assert.NotNull  
    
    /// Asserts that action throws inner exception of certain type.
    static member throwInner<'e when 'e :> exn> (action: unit -> unit) =
        try
            action()
            failwith "Failure! No errors occured."
        with
        | :? AggregateException as aggregateExn ->
            let innerExn = aggregateExn.InnerException 
            let innerExnType = innerExn.GetType()
            Should.equal innerExnType typeof<'e>
    

/// Registers instance as abstraction.
let registerInstanceAs<'i> (instance: obj) (builder: Container) =
    builder.RegisterDelegate<'i>(fun _ -> instance :?> 'i)
    
/// Registers mock instance info builder.
let registerMock<'a when 'a: not struct> (builder: Container) =
    builder.RegisterDelegate<'a>(fun _ -> NSubstitute.Substitute.For<'a>())

/// Single instance LiteDatabase connection.
let connection = new LiteDatabase("MyFeed.db")

/// Injects mocks into container builder.
let registerMocks (builder: Container) =
    builder 
    |> also (registerInstanceAs<LiteDatabase> (new LiteDatabase("_.db")))
    |> also registerMock<IFilePickerService>       
    |> also registerMock<IPlatformService>
    |> also registerMock<INavigationService>
    |> also registerMock<INotificationService>
    |> also registerMock<IPackagingService>
    |> ignore

/// Attribute that cleans database up before and after test.
type CleanUpCollectionAttribute (name: string) =
    inherit BeforeAfterTestAttribute() 
    override __.Before _ = connection.DropCollection name |> ignore
    override __.After _ = connection.DropCollection name |> ignore

/// Deletes the entire file from disk to prevent collisions.
type CleanUpFileAttribute (name: string) =
    inherit BeforeAfterTestAttribute()
    override __.Before _ = File.Delete name      
    override __.After _ = File.Delete name    
     