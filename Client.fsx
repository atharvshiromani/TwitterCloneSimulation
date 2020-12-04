#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote"



open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                deployment {
                    /remoteecho {
                        remote = ""akka.tcp://RemoteFSharp@localhost:8090""
                    }
                }
            }
            remote {
                helios.tcp {
                    port = 0
                    hostname = localhost
                }
                log-dead-letters = 0
                log-dead-letters-during-shutdown = off
            
            debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                    
            }

            }
        }")

let timer =
    System.Diagnostics.Stopwatch()

let topology (mailbox: Actor<_>) =

    let rec loop() = actor {
        let! msg = mailbox.Receive()
        printfn "%s" msg      
    }
    loop()

let system = ActorSystem.Create("RemoteFSharp", configuration)

let numberofusers = 100
timer.Start()
for i in 1..numberofusers do
    let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server")
    let task:Async<obj> = echoClient <? (spawn system (string i) topology )
    let response = Async.RunSynchronously (task)
    printfn "Reply from Server %s" (string(response))


let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server")
let task:Async<obj> = echoClient <? "startsim"
let response = Async.RunSynchronously (task,1000)
printfn "Reply from Server %s" (string(response))

timer.Stop()
printfn "Elapsed Milliseconds: %i" timer.ElapsedMilliseconds
system.Terminate()