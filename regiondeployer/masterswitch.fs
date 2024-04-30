module regiondeployer

open Microsoft.Azure.Management.Fluent
open Microsoft.Azure.Management.AppService.Fluent

[<Literal>]
let public masterfarmername = "chronictick"

let private getfirstmatch(logmessage:string -> unit) : IFunctionApp = 
    let functionapps = getfunctionapps(logmessage)
    let output = Seq.find(fun (currentapp:IFunctionApp) -> currentapp.Name.Contains(masterfarmername)) functionapps    
    output

let togglemasterfarmer (shouldrun:bool, logmessage:string -> unit) : unit =
    String.concat valueseparator ["toggling master farmers operational state to"; shouldrun.ToString()]
    |> logmessage

    let master = getfirstmatch(logmessage)
    
    if master = null then
        String.concat valueseparator [
            "no master farmer found. deploy that first. Its an azure function app whos name must contain"; 
            masterfarmername
        ]
        |> failwith 
        ()

    else
        match shouldrun with
        | true -> master.Start()
        | false -> master.Stop()
        logmessage "masterFarmer toggled."
            
        ()

