namespace regiondeployer

open System
open System.IO

open Microsoft.Azure.Management.AppService.Fluent
open Microsoft.Rest.Azure
open FSharp.Data
open Newtonsoft.Json
open System.Dynamic
open System.Linq
open Microsoft.FSharp.Control
open System.Threading.Tasks
open Microsoft.Azure.Management.ResourceManager.Fluent
open System.Threading

let private crashifnotabsolute inputpath =
    String.concat valueseparator ["checking if"; inputpath; "is absolute"]
        |> logtoconsoleandfile 

    Path.IsPathRooted(inputpath) //only succeeds if path has directory separator or drive letter as its beginning. any further detection is complicated and frankly, not necessary
    |> fun isabsolute -> 
        if not isabsolute then
            String.concat valueseparator ["path"; inputpath; "is not absolute"]
            |> failwith
        
        else
            logtoconsoleandfile "path is absolute" 
         
let private testinputs (inputs: string[]) : Arguments =
    ["validating inputs..."] @ List.ofArray inputs
    |> String.concat valueseparator
    |> logtoconsoleandfile

    match inputs with
    | x when x.Length = 0 -> 
        logtoconsoleandfile noargumentsstring
        exit 0
    | x when x.Length <= 1 && x.[0] = "-h" ->
        logtoconsoleandfile helpstring
        exit 0
    | x when x.Length < 3 || x = null ->
        logtoconsoleandfile noargumentsstring
        exit 0
    | _ -> logtoconsoleandfile "Arguments found"

    let args = { 
        regionalfarmersrootpath = inputs.[0]
        ContractsFolderPath = inputs.[1]
        NumberOfFarmers = int inputs.[2]
    }

    [|inputs.[0]; inputs.[1]|] 
    |> Array.iter crashifnotabsolute
        
    logtoconsoleandfile "inputs validated."
    args

[<EntryPoint>]
let public main (argv:string[]) : int = 
    initialize()
    String.concat valueseparator [DateTime.UtcNow.ToString(); "starting deployment procedure..."]
    |> logtoconsoleandfile 
    
    try
        let arguments = testinputs argv
        let zipfilepath = ZipUpProject (arguments.regionalfarmersrootpath, defaultzipname, logtoconsoleandfile)
        configadapter.initialise();

        let config:configfile = ProjectCredentials.get.matchingcurrentenvironment()

        String.concat valueseparator [
            "deploying using a";
            Enum.GetName(config.currentenvironment.GetType(), config.currentenvironment);
            "environment"
        ]
        |> logtoconsoleandfile

        String.concat valueseparator [
            "using connection string"; 
            config.ConnectionString
        ]
        |> logtoconsoleandfile

        togglemasterfarmer(false, logtoconsoleandfile)

        databasecleaner.removeinitialstate(logtoconsoleandfile)
        deletefarmergroupsmatchingcurrentenvironmentmode logtoconsoleandfile

        databaseseeder.addinitialstate(arguments.ContractsFolderPath, logtoconsoleandfile)
        |> ignore
        
        let group = createrandomresourcegroup(defaultregion, logtoconsoleandfile)
        let storageaccount = createrandomnamedstorageaccount(group, logtoconsoleandfile) //prevents the one thread per deployment creating a new storage account with the same name. it will exist already.

        fixlinuxappbug(group, logtoconsoleandfile, storageaccount)

        let deploymentoutcome = deployarray(
            arguments, 
            zipfilepath, 
            true, 
            group, 
            logtoconsoleandfile, 
            storageaccount
        )

        if deploymentoutcome = true then 
            togglemasterfarmer(true, logtoconsoleandfile)

        else
            logtoconsoleandfile "deploying failed."

    with
    | error ->
        printclouderror(error, logtoconsoleandfile)
        raiseclouderror(error)

    logtoconsoleandfile "procedure finished."
    0
