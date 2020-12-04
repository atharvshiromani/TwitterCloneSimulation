#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Collections.Generic
open System.IO

let mutable client = new ResizeArray<IActorRef>()

let objrandom = System.Random()

let mutable followerlist = new Dictionary<IActorRef, ResizeArray<IActorRef>>()

let mutable hashtagtweets = new Dictionary<string, string>()

let mutable tweets = new Dictionary<IActorRef, ResizeArray<string>>()

let subscribers = new ResizeArray<_>()

let mutable following = new Dictionary<IActorRef, ResizeArray<IActorRef>>()

let config =
    ConfigurationFactory.ParseString(
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 8090
            }
            
            debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                    
            }
            log-dead-letters = 0
            log-dead-letters-during-shutdown = off
        }")


//let maximumfollowers = fsi.CommandLineArgs.[2]

let system = System.create "RemoteFSharp" config

let followers (act: IActorRef, numberofusers:int, maxfollower: int) =
    
    let followernum = maxfollower/objrandom.Next(1,5)

    let mutable follower = new ResizeArray<IActorRef>()
    
    //printfn "%A" act
    //printfn "%i" followernum
    for i in 1..followernum do
        let newfollower = client.[objrandom.Next(0,(numberofusers-1))]
        follower.Add(newfollower)
    

    if followerlist.ContainsKey(act) then
        followerlist.Remove(act) |> ignore
        followerlist.Add(act, follower)
    else
        followerlist.Add(act,follower)

    follower

let retweet (message: string, act: IActorRef) =
    //printfn "------yo%A" tweets
    let followerlist2 = followerlist.Item(act)
    for i in followerlist2 do
        i <! sprintf "Reweeted message : %s from its subsriber feed " message
        //printfn "%A retweeted %s to follower %A" act message i


let sendTweet (act: IActorRef, followerlist: Dictionary<IActorRef,ResizeArray<_>>) =
    //printfn "------"
    //follower |> Seq.iteri (fun index item -> printfn "%i: %A" index follower.[index])
    let tweet = "Its the endgame now #AvengersEndGame" 
    let usertweet ="Tony Stark @ironman"
    let templist = followerlist.Item(act)
    let templist2 = new ResizeArray<_>()
    templist2.Add(tweet)
    templist2.Add(usertweet)
    tweets.Add(act, templist2)
    

    hashtagtweets.Add("#AvengersEndGame",tweet)
    hashtagtweets.Add("@ironman",usertweet)

    for i in templist do
        i <! sprintf "This is the original tweet: %s" tweet
        //printfn "%A tweeted %s to follower %A" act tweet i
        retweet(tweet, i)

let subscriberList (act : IActorRef) =
    //printfn "Getting the subsrciber list"
    // let templist = followers (act)
    
    // for i in templist do
    //     let templist2 = followers (i)
    //     for j in templist2 do
    //         if act.Equals(j) then
    //             subscribers.Add()
    
    for i in client do
        let templist = followerlist.Item(i)
        //printfn "%A" templist
        for j in templist do
            if act.Equals(j) then
                subscribers.Add(i)
    //printfn "THE SUBSCRIBERLIST IS %A" subscribers

    if following.ContainsKey(act) then
        following.Remove(act)
        following.Add(act, subscribers)
    else
        following.Add(act,subscribers)
    
    subscribers
   
let queringfunction(query: string, act: IActorRef) =
 
    let temptweet = hashtagtweets.Item(query)
    //let templist = following.Item(act)
    printfn "The tweet with %s are: %s" query temptweet
    //temptweet
    




let register (act: IActorRef) =
    client.Add(act)
    
// let tail = Seq.last client

// numberofusers <- client.FindIndex (fun s -> s = tail)     

let echoServer = 
    spawn system "Server"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? IActorRef as node-> 
                    register(node)
                    
                    sender <! sprintf "%A registered succesfully" node 
                
                | :? ResizeArray<int32> as userinput ->

                    sender <! sprintf "Intiating Simulation Sequence" 
                    let numberofusers = userinput.[0]
                    let maxfollower = userinput.[1]
                    let node = client.[2]
                    for i in client do
                        followers(i, numberofusers, maxfollower) |> ignore
                    sendTweet(node, followerlist)
                    //subscriberList(node) |>ignore
                    queringfunction("#AvengersEndGame",node)
                    queringfunction("@ironman",node)
                    //sender <! "Simulation Complete" 

                return! loop()
                
        
               
            }
        loop()

Console.ReadLine() |> ignore