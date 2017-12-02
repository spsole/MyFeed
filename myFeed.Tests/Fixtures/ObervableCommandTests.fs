module myFeed.Tests.Fixtures.ObservableCommandTests

open Xunit
open myFeed.Tests.Extensions
open myFeed.ViewModels.Bindables
open System.Threading.Tasks

[<Fact>]
let ``should execute passed actions``() =

    let mutable fired = 0
    let command = ObservableCommand(fun () -> fired <- fired + 1; Task.CompletedTask)
    command.Execute()
    Should.equal true (command.CanExecute()) 
    Should.equal 1 fired

[<Fact>]
let ``should await until command execution is finished``() =

    let command = ObservableCommand(fun () -> Task.Delay(100))
    command.Execute()
    Should.equal false (command.CanExecute())

[<Fact>]
let ``should raise state change event``() =

    let mutable fired = 0
    let command = ObservableCommand(fun () -> Task.CompletedTask)
    command.CanExecuteChanged += fun _ -> fired <- fired + 1
    command.Execute()
    Should.equal 2 fired 