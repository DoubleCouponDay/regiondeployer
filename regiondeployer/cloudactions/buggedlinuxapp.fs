module regiondeployer.cloudactions.buggedlinuxapp

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
open regiondeployer.constants.pathconstants
open Newtonsoft.Json
open serversidearchitecture.common
open regiondeployer.cloudactions.azurefunction
open System.Collections.Generic
open Microsoft.Azure.Management.AppService.Fluent.AppServicePlan.Definition
open regiondeployer.cloudactions.serviceplan

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