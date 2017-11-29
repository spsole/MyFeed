[<AutoOpen>]
module myFeed.Tests.Extensions.Default

open Xunit
open Autofac
open System
open System.Linq
open System.Reflection
open NSubstitute

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
    static member resolve<'a> (scope: ILifetimeScope) = scope.Resolve<'a>() |> Assert.NotNull  
    
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
    