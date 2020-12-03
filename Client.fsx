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
            }
        }")

let topology (mailbox: Actor<_>) =

    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let mutable y = 0
        y = y + 1      
    }
    loop()

let system = ActorSystem.Create("RemoteFSharp", configuration)

let numberofusers = 100
let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server")
for i in 1..numberofusers do

    
    let task:Async<obj> = echoClient <? (spawn system (string i) topology)
    let response = Async.RunSynchronously (task,1000)
    printfn "Reply from Server %s" (string(response))


let echoClient1 = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server1")
let task1:Async<obj> = echoClient1 <? "startsim"
let response = Async.RunSynchronously (task1,1000)
printfn "Reply from Server %s" (string(response))