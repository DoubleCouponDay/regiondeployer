namespace regiondeployer

open System
open System.IO

let private filtercontractthroughtests (file:string, logmessage:string -> unit) : exchangecontract.Root option =
    String.concat valueseparator ["filtering contract through tests"; file]
    |> logmessage 

    match file.EndsWith(".json") with
    | true ->
       try
            exchangecontract.Load((file))  
            |> fun contract ->
                if contract.Info.Contractname <> String.Empty then
                    logmessage "contract passed tests."
                    Some(contract)

                else
                    logmessage "contract is empty."
                    None

       with
       | error1 ->
            logmessage "contract could not be loaded."
            None //some json files in the contract directory are not yet valid json
       
    | false ->
        logmessage "file is not a json file."
        None

let public getvalidcontractfiles (folderpath:string, logmessage:string -> unit) : seq<exchangecontract.Root> =
    logmessage "getting valid contract files..."
    let files = Directory.EnumerateFiles(folderpath) 

    seq {
        for file in files do 
            let testoutcome = filtercontractthroughtests(file, logmessage)

            if testoutcome.IsSome then     
                yield testoutcome.Value
    }