namespace myFeed.Tests.Logging

open System
open System.Reflection

open Moq
open Xunit.Sdk

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore.Storage

[<AutoOpen>]
module Logging =

    [<Literal>] 
    /// True for spammy output.
    let LoggingEnabled = false

    /// Logs anything to the console.
    let log (object: obj) = 
        if (LoggingEnabled) then
            string object
            |> sprintf "[myFeed.Tests] %s" 
            |> Console.WriteLine

/// Displays test name before test is executed.
type LogAttribute() =
    inherit BeforeAfterTestAttribute() with
        override x.Before(info: MethodInfo) =
            sprintf "(Starting Test): %s" info.Name |> log
        override x.After(info: MethodInfo) =
            sprintf "(Finishing Test): %s" info.Name |> log

/// Logger that processes EntityFramework 
/// debug sql output for XUnit.
type XUnitLogger() =
    interface ILogger with
        member x.IsEnabled(logLevel) = true
        member x.BeginScope(state: 'a) = 
            new Mock<IDisposable>() |> fun mock -> mock.Object
        member x.Log(level, eventId, state: 'a, ex, formatter) =
            if (not <| Object.Equals(state, null)) then 
                match level with
                | LogLevel.Debug -> ()
                | LogLevel.Information -> 
                    match box state with 
                    | :? DbCommandLogData as cmd ->
                        let query = cmd.CommandText
                        if ("PRAGMA" 
                            |> query.Contains 
                            |> not) 
                        then
                            query.Replace("\r\n", " ") 
                            |> log
                    | _ -> ()
                | _ -> ()

/// Logger provider providing XUnitLogger.
type XUnitLoggerProvider() =
    interface ILoggerProvider with
        member x.Dispose() = ()
        member x.CreateLogger(categoryName: string) = 
            XUnitLogger() :> ILogger