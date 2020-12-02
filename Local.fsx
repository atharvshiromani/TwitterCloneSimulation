//LocalActor.fsx
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



let system = ActorSystem.Create("RemoteFSharp", configuration)
let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@localhost:8090/user/Server")
let task:Async<obj> = echoClient <? "F#!"
let response = Async.RunSynchronously (task,1000)
printfn "Reply from remote %s" (string(response))