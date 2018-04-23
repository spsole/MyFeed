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
open NSubstitute
open System.Reactive

let toList (sequence: seq<'a>) = sequence.ToList()

let dispose (disposable: IDisposable) = disposable.Dispose()

let inline (+=) (event: IEvent<'a, 'b>) (handler: 'b -> unit) = event.Add handler

let inline also defun arguments = defun arguments; arguments

let fst (first, _) = first 

let snd (_, second) = second   

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

type ICommand with 

    member command.Invoke() =
        let source = TaskCompletionSource<bool>()
        command.CanExecuteChanged += fun _ -> 
            if command.CanExecute() then 
                source.SetResult(true) |> ignore
        command.Execute(Unit.Default)       
        source.Task      

type Should = 
    
    static member beNull<'a> (a: 'a) = Assert.Null(a)
    
    static member notBeNull<'a> (a: 'a) = Assert.NotNull(a)
    
    static member equal<'a> (a: 'a) (b: 'a) = Assert.Equal(a, b)
    
    static member notEqual<'a> (a: 'a) (b: 'a) = Assert.NotEqual(a, b)
    
    static member contain (a: string) (b: string) = Assert.Contains(a, b)
        
    static member throw<'e when 'e :> exn> act = Assert.Throws<'e>(Action(act))   
    
    static member notBeEmpty<'a, 'b when 'a :> seq<'b>> (ls: 'a) = Assert.NotEmpty   
    
    static member resolve<'a> (scope: IResolver) = scope.Resolve<'a>() |> Assert.NotNull  
    
    static member throwInner<'e when 'e :> exn> (action: unit -> unit) =
        try
            action()
            failwith "Failure! No errors occured."
        with
        | :? AggregateException as aggregateExn ->
            let innerExn = aggregateExn.InnerException 
            let innerExnType = innerExn.GetType()
            Should.equal innerExnType typeof<'e>
    
let connection = new LiteDatabase("MyFeed.db")

type CleanUpCollectionAttribute (name: string) =
    inherit BeforeAfterTestAttribute() 
    override __.Before _ = connection.DropCollection name |> ignore
    override __.After _ = connection.DropCollection name |> ignore

type CleanUpFileAttribute (name: string) =
    inherit BeforeAfterTestAttribute()
    override __.Before _ = File.Delete name      
    override __.After _ = File.Delete name    
     