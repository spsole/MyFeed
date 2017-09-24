namespace myFeed.Tests.Logging

open System
open System.Reflection

open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore.Storage

open Xunit.Sdk
open Moq

/// XUnit logger for queries logging.
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
                        if ("PRAGMA" |> query.Contains |> not) 
                        then query.Replace("\r\n", " ") 
                            |> sprintf "[myFeed.Tests] %s" 
                            |> Console.WriteLine
                    | _ -> ()
                | _ -> ()

/// XUnit logger provider for query logging.
type XUnitLoggerProvider() =
    interface ILoggerProvider with
        member x.Dispose() = ()
        member x.CreateLogger(categoryName: string) = 
            XUnitLogger() :> ILogger