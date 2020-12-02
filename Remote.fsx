// RemoteActor.fsx
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Collections.Generic
open System.IO

let config =
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 8090
            }
        }"


let system = System.create "RemoteFSharp" config
let echoServer = 
    spawn system "Server"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                printfn "echoServer called"
                match box message with
                | :? string -> 
                    sender <! sprintf "Echo: %s" message
                    return! loop()
                   
                | _ ->  failwith "Unknown message"
            }
        loop()
Console.ReadLine() |> ignore