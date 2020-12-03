#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Collections.Generic
open System.IO

let mutable client = new ResizeArray<IActorRef>()

let objrandom = new System.Random()

let mutable followerlist1 = new ResizeArray<IActorRef>()

let mutable followerlist = new Dictionary<IActorRef, ResizeArray<IActorRef>>()

let mutable tweets = new Dictionary<IActorRef, string>()

let subscribers = new ResizeArray<_>()

let mutable following = new Dictionary<IActorRef, ResizeArray<IActorRef>>()

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

let followers (act: IActorRef) =
    
    printfn "-----follower"
    let followernum = 5

    let mutable follower = new ResizeArray<IActorRef>()
    
    //printfn "%A" act
    //printfn "%i" followernum
    for i in 1..followernum do
        let newfollower = client.[objrandom.Next(0,99)]
        follower.Add(newfollower)
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
    follower |> Seq.iteri (fun index item -> printfn "%i: %A" index follower.[index])
    let tweet = "i love india #india" 
    tweets.Add(act, tweet)
    for i in follower do
        i <! tweet
        printfn "%A tweeted %s to follower %A" act tweet i
        retweet(tweet, i)

let subscriberList (act : IActorRef) =
    printfn "Getting the subsrciber list"
    // let templist = followers (act)
    
    // for i in templist do
    //     let templist2 = followers (i)
    //     for j in templist2 do
    //         if act.Equals(j) then
    //             subscribers.Add()
    
    for i in client do
        let templist = followers (i)
        for j in templist do
            if act.Equals(j) then
                subscribers.Add(i)
    printfn "THE SUBSCRIBERLIST IS %A" subscribers

    if following.ContainsKey(act) then
        following.Remove(act)
        following.Add(act, subscribers)
    
    subscribers
   

let register (act: IActorRef) =
    client.Add(act)
    printfn "Actor %A registered" act
    

let echoServer = 
    spawn system "Server"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? IActorRef -> 
                    register(message)
                    
                    sender <! sprintf "%A" message
                    
                    return! loop()

        
               
            }
        loop()

let echoServer1 = 
    spawn system "Server1"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string -> 
                    sender <! sprintf "Now Intiating Simulation Sequence"
                    let node = client.[objrandom.Next(0,99)]
                    followers(node)
                    sendTweet(node, followers (node))
                    subscriberList(node)
                    return! loop()
            }
        loop()



Console.ReadLine() |> ignore