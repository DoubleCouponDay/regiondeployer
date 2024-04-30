namespace regiondeployer

open System
open System.IO
open Microsoft.FSharp.Core

type private logginglock() =
    static member val public lock = new Object()
        with get

let private logfile : string = (Directory.GetCurrentDirectory() + "/log.txt")

let public initialize() : unit = 
    use init = File.Create(logfile)
    ()

///logs to console and local file
///does not work in a unit test
let public logtoconsoleandfile (message:string) : unit =
    lock logginglock.lock (fun _ -> 
        Console.WriteLine message
        use logfilestream = File.AppendText(logfile)
        logfilestream.WriteLine(message)
    )
    
