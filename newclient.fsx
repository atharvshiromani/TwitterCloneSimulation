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
let objrandom = new System.Random()

let usersnumber = 100

let mutable client = new ResizeArray<IActorRef>()

let mutable followerlist1 = new ResizeArray<IActorRef>()

let mutable followerlist = new Dictionary<IActorRef, ResizeArray<IActorRef>>()
//let mutable tweetlist = new Dictionary<IActorRef, ResizeArray<String>>()
//type ProcessorMessage = ProcessJob1 of IActorRef * int
let mutable tweets = new Dictionary<IActorRef, string>()




let followers (act: IActorRef) =
    
    printfn "-----follower"
    let followernum = objrandom.Next(1,99)

    let mutable follower = new ResizeArray<IActorRef>()
    
    //printfn "%A" act
    //printfn "%i" followernum
    for i in 1..followernum do
        follower.Add(client.[objrandom.Next(0,99)])
    followerlist1 <- follower

    if followerlist.ContainsKey(act) then
        followerlist.Remove(act)
        followerlist.Add(act, followerlist1)
    followerlist1

let retweet (message: string, act: IActorRef) =
    //printfn "------yo%A" tweets
    let followerlist2 = followers(act)
    for i in followerlist2 do
        i <! message
        printfn "%A retweeted %s to follower %A" act message i


let sendTweet (act: IActorRef, follower: ResizeArray<_>) =
    printfn "------"
    let tweet = "i love india #india" 
    tweets.Add(act, tweet)
    for i in follower do
        i <! tweet
        printfn "%A tweeted %s to follower %A" act tweet i
        retweet(tweet, i)
   
    //printfn "tweetlist %A" tweets
    
// let reTweet (act: IActorRef) =
//     let retweet = "Sending reTweet"    
        
// let queringfunc () =

let topology (mailbox: Actor<_>) =

    let rec loop() = actor {
        let! (msg, node: IActorRef) = mailbox.Receive()
        Akka.Dispatch.ActorTaskScheduler.RunTask(fun () ->
        async {
            //let follower = followers(node)
            match msg,node with 
            | "follow",node -> followers(node) |> ignore 
            | "sendtweet",node-> sendTweet(node,followers (node))         
        } |> Async.StartAsTask :> Threading.Tasks.Task)
    }
    loop()

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

let register (number: int) =
    client.Add(echoServer)

  
for i in 1..usersnumber do
    register(i)


//client |> Seq.iteri (fun index item -> printfn "%i: %A" index client.[index])

for i in 0..usersnumber-1 do
    followerlist.Add(client.[i], null )

// for i in client do
//     i <! ("follow", i)

let startnode  = client.[2]
printfn "%A" startnode
startnode <! ("sendtweet",startnode)





Console.ReadLine() |> ignore