namespace regiondeployer

open Microsoft.Azure.Management.AppService.Fluent
open regiondeployer
open System.Configuration
open System.Linq

open Microsoft.Azure.Management.Storage.Fluent
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
open System
open System.Collections.Generic
open Microsoft.Azure.Management.Fluent
open Microsoft.Azure.Management.WebSites
open MoonMachine.credentials

type private listlock() = 
    static member val resource:obj = new obj() with get

type farmer() =
    [<Literal>]
    let basefarmername = "mmregionalfarmer"

    [<Literal>]
    let maxfarmernamelength = 40

    member public this.getappsettings() : Dictionary<string, string> =
        let output = new Dictionary<string, string>()
        output.Add("FUNCTIONS_EXTENSION_VERSION", "~2")
        output.Add("AzureWebJobsSecretStorageType", "Files")
        output.Add("WEBSITE_RUN_FROM_PACKAGE", "1")
        output.Add("WEBSITE_NODE_DEFAULT_VERSION", "8.11.1")
        output
        

    member public this.getfunctionapps(logmessage:string -> unit): IEnumerable<IFunctionApp> = 
        let gateway = createauthenticatedgateway()

        lock listlock.resource (fun _ ->        
            gateway.AppServices
                .FunctionApps
                .List()
        ) 

    ///must be executed synchronously
    member public this.getrandomfarmername(logmessage:string -> unit) : string =
        logmessage "getting random farmer name..."
        let randomname = createrandomname(basefarmername, maxfarmernamelength)
        let functionapps = getfunctionapps(logmessage)

        let namechecked = 
            functionapps.Where(fun app -> app.Name = randomname)
                .FirstOrDefault()

        String.concat valueseparator ["checking name is unique"; randomname]
        |> logmessage

        if namechecked = null then
            logmessage "name is unique"
            randomname

        else
            logmessage "name is not unique. recursing..."
            getrandomfarmername logmessage

    member private this.converthostnametoip (hostname:string) : string = //dns lookup using an external api
        let globalconfig = ProjectCredentials.get.matchingcurrentenvironment()

        let accesskey = globalconfig.IpStackKey
        let url = String.concat String.Empty ["http://api.ipstack.com/"; hostname; accesskeyparamkey; accesskey]
        let response = ipstack.Load(url)
        response.Ip

    member public this.externalipalreadyexistsingroup (hostnametotest:string, resourcegroupname:string, logmessage:string -> unit) : bool =  
        let questionappsip = converthostnametoip hostnametotest

        String.concat valueseparator ["checking that farmers external ip "; questionappsip; "does not already exist in resource group"; resourcegroupname]
        |> logmessage

        let groupandhostnamematch (someapp:IFunctionApp) : bool = 
            let queryappsip = converthostnametoip someapp.DefaultHostName
            
            someapp.ResourceGroupName = resourcegroupname && //assuming that function apps cannot have the same external ip unless they exist in the same region.
            questionappsip = queryappsip

        let query =
            getfunctionapps(logmessage)
                .Where(groupandhostnamematch)   
                
        query
        |> fun result -> 
            let existingcount = result.Count()

            match existingcount with
            | 1 -> //assuming there you need an existing function app to get a hostname. 
                String.concat valueseparator ["farmers ip"; questionappsip; "is unique."] 
                |> logmessage           
                false

            | _ -> 
                String.concat valueseparator ["farmers ip"; questionappsip; "already exists"; existingcount.ToString(); "times."]
                logmessage("farmers ip already exists.")
                true

    ///returns a real functionapp. requires existing resourcegroupname.
    ///ensures that any existing farmers, with the same region as the created one, are guaranteed to have different external IP addresses.
    ///created farmer will be on.
    member public this.createfarmer (appname:string, regionname:string, existingresourcegroupname:string, existingstorageaccount:IStorageAccount, logmessage:string -> unit) : IFunctionApp =
        String.concat valueseparator ["creating definition of farmer..."; appname; regionname]
        |> logmessage

        let gateway = createauthenticatedgateway()

        let output = (gateway.AppServices
            .FunctionApps
            .Define(appname)
            .WithRegion(regionname)
            .WithExistingResourceGroup(existingresourcegroupname) //name will be in the format farmername + "group"
            .WithExistingStorageAccount(existingstorageaccount)
            .WithNewConsumptionPlan()
            .WithAppSettings(getappsettings())        
            .WithHttpsOnly(true)
            .WithContainerLoggingEnabled()
            .WithRemoteDebuggingDisabled()
            .Create()) 

        String.concat valueseparator ["definition created"; output.ToString()]
        |> logmessage
        output.Start()

        output

    member public this.deletesinglefarmer(farmerid:string, logmessage:string -> unit) : unit =
        String.concat valueseparator [
            "deleting farmer of id";
            farmerid
        ]
        |> logmessage
        let gateway = createauthenticatedgateway()

        gateway.AppServices
            .FunctionApps
            .DeleteById(farmerid)

        logmessage "farmer deleted."
        ()

    ///deletes any occurrance of a farmer matching input farmer name.
    member public this.deletefarmers (farmername:string, logmessage:string -> unit) : int =    
        let mutable farmersdeleted = 0

        getfunctionapps(logmessage)
            .Where(
                fun farmer -> farmer.Name = farmername
            )
            .Select(
                fun matchingfarmer ->
                    deletesinglefarmer(matchingfarmer.Id, logmessage)
                    farmersdeleted <- (farmersdeleted + 1)
            )
        |> ignore
        farmersdeleted
