module regiondeployer

open Microsoft.Azure.Management.ResourceManager.Fluent
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
open Microsoft.Azure.Management.Fluent
open System
open System.Linq
open Microsoft.Azure.Management.ResourceManager.Fluent.Models
open Microsoft.Azure.Management.AppService.Fluent
open System.Configuration
open System.Net.Http
open System.IO.Compression
open System.IO
open System.Text
open Microsoft.Rest.Azure
open System.Threading.Tasks
open System.Threading
open Microsoft.Azure.Management.Storage.Fluent
open Newtonsoft.Json
open serversidearchitecture.common
open System.Collections.Generic
open Microsoft.Azure.Management.AppService.Fluent.AppServicePlan.Definition

let public fixlinuxappbug(resourcegroupname:string, logmessage:string -> unit, storageaccount:IStorageAccount) : unit =
    logmessage "fixing linux app bug..."
    let azuregateway = createauthenticatedgateway()
    let appname = basefarmername + buggedserviceplanname
    let appsettings:Dictionary<string,string> = getappsettings()
    let appplan = createlinuxserviceplan(resourcegroupname, logmessage)
    logmessage ("creating bugged farmer: " + appname)

    let buggedapp =
        azuregateway.AppServices
            .FunctionApps
            .Define(appname)
            .WithExistingAppServicePlan(appplan)
            .WithExistingResourceGroup(resourcegroupname)
            .WithExistingStorageAccount(storageaccount)
            .WithAppSettings(appsettings)
            .Create()

    deletesinglefarmer(buggedapp.Id, logmessage)
    deleteserviceplan(appplan.Id, logmessage)
    
    logmessage "linux app bug fixed?"
    ()