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

for i in 1..numberofusers do

    let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server")
    let task:Async<obj> = echoClient <? (spawn system (string i) topology)
    let response = Async.RunSynchronously (task,1000)
    printfn "Reply from remote %s" (string(response))