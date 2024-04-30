module regiondeployer

open Microsoft.Azure.Management.AppService.Fluent
open Microsoft.Azure.Management.Fluent
open System
open regiondeployer
open System.Configuration
open System.Linq
open Microsoft.Azure.Management.Storage.Fluent

open System.Threading
open Microsoft.FSharp.Control
open System.Threading.Tasks
open Microsoft.Azure.Management.ResourceManager.Fluent
open regiondeployer
open randomnamegenerator
open regiondeployer
open MoonMachine.credentials
open MoonMachine.Infrastructure.models
open MoonMachine.Core.interop.models

[<Literal>]
let public maxgroupnamelength = 90

[<Literal>]
let public basegroupname = "mmfarmergroup"

[<Literal>]
let public defaultregion = "westus2"

let private groupnameisavailable (groupname:string, logmessage:string -> unit) : bool = 
    let gateway = createauthenticatedgateway()
    gateway.ResourceGroups
        .Contain(groupname) = false //fixed nasty bug where it would always return true if group was unavilable

let public deleteresourcegroup (resourcegroupname : string, logmessage:string -> unit) : unit =
    let gateway = createauthenticatedgateway()
    String.concat valueseparator ["deleting group"; resourcegroupname]
        |> logmessage

    gateway.ResourceGroups.DeleteByName(resourcegroupname)
    logmessage("group deleted.")    

let private getbaseenvironmentname() : string =
    let baseenvironmentname:mmenvironment = ProjectCredentials.get.matchingcurrentenvironment().currentenvironment
    let name:string = basegroupname + Enum.GetName(baseenvironmentname.GetType(), baseenvironmentname)
    name

let public deletefarmergroupsmatchingcurrentenvironmentmode (logmessage:string -> unit) : unit =
    let gateway = createauthenticatedgateway()
    let name:string = getbaseenvironmentname()

    let matchinggroups = (
        gateway.ResourceGroups
            .List()
            .Where(fun group -> 
                group.Name
                    .ToLower()
                    .Contains(name)
            )
    )
        
    String.concat valueseparator ["deleting old farmer groups with names containing"; name]
    |> logmessage

    let mutable groupsdeleted = 1 //needs to be 1 indexed since im waiting for this to be equal to matchinggroups count

    Parallel.ForEach(matchinggroups, fun (group:IResourceGroup) -> 
        deleteresourcegroup (group.Name, logmessage)
        groupsdeleted <- (groupsdeleted + 1)
    )
    |> fun loopstate ->
        while groupsdeleted < matchinggroups.Count() do
            logmessage "waiting for parallel delete operations to finish..."
            Thread.Sleep(TimeSpan.FromSeconds(1.0))

        logmessage("groups deleted.")

let rec private getavailablerandomname(logmessage:string -> unit) =
    let basename = getbaseenvironmentname()
    let output = createrandomname(basename, maxgroupnamelength)

    if groupnameisavailable(output, logmessage) then
        output

    else
        getavailablerandomname logmessage
        
let public createrandomresourcegroup (regionname:string, logmessage:string -> unit) : string =        
    let gateway = createauthenticatedgateway()
    let randomgroupname = getavailablerandomname logmessage

    String.concat valueseparator ["creating resource group"; randomgroupname]
    |> logmessage 

    let output = 
        gateway.ResourceGroups
            .Define(randomgroupname)
            .WithRegion(regionname) //does it really need this?
            .Create()
            .Name

    logmessage "resource group created."
    output
