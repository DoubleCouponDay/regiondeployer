module regiondeployer

open Microsoft.Azure.Management.ResourceManager.Fluent
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
open Microsoft.Azure.Management.Fluent
open System
open System.Linq
open Microsoft.Azure.Management.AppService.Fluent
open System.Configuration
open regiondeployer
open Microsoft.Azure.Management.Storage.Fluent.Models
open Microsoft.Azure.Management.Storage.Fluent

[<Literal>]
let public defaultstoragename = "mmstorage" 

[<Literal>]
let public maxstoragenamesize = 24

///uses the default region since storage isnt available in all the functionapp regions
let private createstorageaccountinresourcegroup (storageaccountname:string, resourcegroupname:string, logmessage:string -> unit) : IStorageAccount =
    let gateway = createauthenticatedgateway()
    String.concat valueseparator ["creating storage account with existing group"; resourcegroupname]
    |> logmessage

    let output =
        gateway.StorageAccounts
            .Define(storageaccountname)
            .WithRegion(defaultregion)
            .WithExistingResourceGroup(resourcegroupname)            
            .WithGeneralPurposeAccountKindV2()            
            .WithOnlyHttpsTraffic()                        
            .WithSku(StorageAccountSkuType.Standard_LRS) //LRS is the cheapest redundancy strategy            
            .Create()

    logmessage "storage account created."
    output
            
let rec public createrandomnamedstorageaccount (resourcegroupname:string, logmessage:string -> unit) : IStorageAccount =
    let gateway = createauthenticatedgateway()
    let randomname = createrandomname(defaultstoragename, maxstoragenamesize)
    let namechecked = gateway.StorageAccounts.CheckNameAvailability(randomname)

    if namechecked.IsAvailable.HasValue &&
        namechecked.IsAvailable.Value then
        createstorageaccountinresourcegroup(randomname, resourcegroupname, logmessage)

    else
        createrandomnamedstorageaccount(resourcegroupname, logmessage)


