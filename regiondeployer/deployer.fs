module regiondeployer.deployer

open Microsoft.Azure.Management.ResourceManager.Fluent
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
open Microsoft.Azure.Management.Fluent
open System
open System.Linq
open Microsoft.Azure.Management.ResourceManager.Fluent.Models
open Microsoft.Azure.Management.AppService.Fluent
open System.Configuration
open regiondeployer.constants.independentconstants
open regiondeployer.cloudactions.azurefunction.farmer
open regiondeployer.cloudactions.storage
open System.Net.Http
open System.IO.Compression
open System.IO
open System.Text
open regiondeployer.azureauthenticator
open Microsoft.Rest.Azure
open regiondeployer.cloudactions.resourcegroup
open regiondeployer.types
open regiondeployer.databaseseeder
open System.Threading.Tasks
open System.Threading
open Microsoft.Azure.Management.Storage.Fluent
open regiondeployer
open regiondeployer.randomnamegenerator
open regiondeployer.clouderror
open regiondeployer.constants.pathconstants
open Newtonsoft.Json
open serversidearchitecture.common
open System.Collections.Generic
open System.Net.Http.Headers

let private concurrentdeploymentslimit = azureregions.Load(azureregionsjsonpath).Length

let private postdeploymentrequest (appname:string, zipfilepath:string, username:string, password:string, logmessage:string -> unit) : HttpResponseMessage =   
        use http = new HttpClient()
        logmessage "making post request to apps kudu interface..."
        let filesbytes = File.ReadAllBytes(zipfilepath)
        let httpcontent = new ByteArrayContent(filesbytes)
        httpcontent.Headers.ContentType <- new MediaTypeHeaderValue("application/octet-stream")
        let basicauthpair : KeyValuePair<string, string> = http.createbasicauthheader(username, password)  

        let address = 
            String.concat 
                String.Empty [@"https://"; appname; ".scm.azurewebsites.net/api/zipdeploy"]

        let requestmessage:HttpRequestMessage = 
            httputilities.createrequestmessage(address, HttpMethod.Post, httpcontent, basicauthpair)

        let output = 
            (globalhttpport.get
                .client
                .SendAsync(requestmessage)
                .Result)

        requestmessage.Dispose()

        logmessage "response received."
        output

///returns the creates azure resource and the status of deployment
let public deployone (appname:string, regionname:string, zipfilepath:string, resourcegroupname:string, storageaccount:IStorageAccount, logmessage:string -> unit) : (IFunctionApp * bool) =    
    let farmer = createfarmer(appname, regionname, resourcegroupname, storageaccount, logmessage)
    farmer.Stop() //should prevent file copy locking issues. preventative programming.

    String.concat valueseparator ["deploying regional farmer..."; appname; regionname]
    |> logmessage

    let publishingprofile = farmer.GetPublishingProfile()
    let username = publishingprofile.GitUsername
    let password = publishingprofile.GitPassword

    let response = postdeploymentrequest(farmer.Name, zipfilepath, username, password, logmessage)

    if response.IsSuccessStatusCode = false then
        String.concat valueseparator [
            "deployment failed for farmer"; 
            farmer.Name; 
            response.StatusCode.ToString(); 
            response.ReasonPhrase;            
        ]
        |> logmessage

    else
        logmessage (
            String.concat valueseparator [ "deployed to regional farmer."; regionname]
        )
    farmer.Start()
    response.Dispose()
    (farmer, response.IsSuccessStatusCode)

let public deployarray (inputs:Arguments, zipfilepath:string, failonerror:bool, resourcegroupname:string, logmessage:string -> unit, storageaccount:IStorageAccount) : bool =
    String.concat valueseparator [ 
        "deploying the array of regional farmers..."; 
        inputs.NumberOfFarmers.ToString(); 
        "long."]
    |> logmessage
    
    let regions = azureregions.Load(azureregionsjsonpath)
    
    let mutable deploymentindex = 0
    let mutable successfuldeployments = 0

    let deployfarmersinspecificrange (startindexinclusive:int) (endindexexclusive:int) (inputs:Arguments) : ParallelLoopResult =
        Parallel.For(startindexinclusive, endindexexclusive, fun i state ->    
            try
                logmessage (
                    String.concat 
                        valueseparator 
                        ["deploying farmer"; i.ToString()]
                )

                let currentregion =
                    if i < regions.Length then
                        regions.[i].Name //somehow azure converts input Name into output Display Name. I dont think its worth risking testing input display name.

                    else
                        defaultregion

                let randomfarmername = 
                    (getrandomfarmername logmessage)

                deployone(            
                    randomfarmername,
                    currentregion,
                    zipfilepath,         
                    resourcegroupname,
                    storageaccount,
                    logmessage
                )
                |> fun resultwip -> match resultwip with
                | (createdresource, deploymentstatus) ->    
                    let masterkey = createdresource.GetMasterKey()
                    databaseseeder.seednewregionalfarmer (createdresource, masterkey, logmessage)
                    String.concat valueseparator ["regionfarmer"; i.ToString(); "deployed."; currentregion]
                    |> logmessage 

                    if deploymentstatus = true then
                        successfuldeployments <- successfuldeployments + 1                

            with 
            | error1 -> //parallel.for smothers errors occuring in its worker threads
                if failonerror then
                    printclouderror(error1, logmessage)
                    raiseclouderror error1                    
        
                else
                    printclouderror(error1, logmessage)
                    
                ()
        )    
    
    try
        while deploymentindex < inputs.NumberOfFarmers do
            let slotsize = 
                let workleft = inputs.NumberOfFarmers - deploymentindex

                if workleft < concurrentdeploymentslimit then 
                    workleft

                else
                    concurrentdeploymentslimit

            let slotendindex = deploymentindex + slotsize

            String.concat valueseparator ["deploying farmers"; deploymentindex.ToString(); "up to"; slotendindex.ToString()]
            |> logmessage

            deployfarmersinspecificrange deploymentindex slotendindex inputs
            |> fun result ->    
                deploymentindex <- slotendindex //the end index becomes the new index since it was excluded in the previous rangedeploy

        String.concat valueseparator [
            "array deployed. there were"; 
            successfuldeployments.ToString(); 
            "/";
            inputs.NumberOfFarmers.ToString();
            "successful deployments."
        ]
        |> logmessage
        true

    with
    | error1 -> 
        if failonerror then
            printclouderror(error1, logmessage)
            raiseclouderror error1
            false
        
        else
            printclouderror(error1, logmessage)
            false


