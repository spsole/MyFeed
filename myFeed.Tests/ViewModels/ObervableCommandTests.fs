module myFeed.Tests.ViewModels.ObservableCommandTests

open Xunit
open myFeed.Tests.Extensions
open myFeed.ViewModels.Bindables
open System.Threading.Tasks
open System

[<Fact>]
let ``should execute passed actions``() =

    let mutable fired = 0
    let command = Func<Task>(fun () -> 
        fired <- fired + 1 
        Task.CompletedTask) |> ObservableCommand
    command.Execute()
    Should.equal true (command.CanExecute()) 
    Should.equal 1 fired

[<Fact>]
let ``should await previous execution``() =

    let command = Func<Task>(fun () -> Task.Delay(1000)) |> ObservableCommand
    command.Execute()
    Should.equal false (command.CanExecute())

[<Fact>]
let ``should raise state change event``() =

    let mutable fired = 0
    let command = Func<Task>(fun () -> Task.CompletedTask) |> ObservableCommand
    command.CanExecuteChanged += fun _ -> fired <- fired + 1
    command.Execute()
    Should.equal 2 fired 