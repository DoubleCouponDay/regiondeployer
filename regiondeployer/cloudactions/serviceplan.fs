namespace regiondeployer

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

[<Literal>]
let public buggedserviceplanname = "buggedappservice"

[<Literal>]
let public defaultlinuxregion = "westus"

///SimonLuckenuik commented on Nov 17, 2018
///https://github.com/Azure/azure-libraries-for-java/issues/536#issuecomment-439585631
let public createlinuxserviceplan(resourcegroupname:string, logmessage:string -> unit) : IAppServicePlan =
    String.concat valueseparator [
        "creating app service plan of name";
        buggedserviceplanname
    ]
    |> logmessage

    let azuregateway = createauthenticatedgateway()

    let output = 
        azuregateway.AppServices
            .AppServicePlans
            .Define(buggedserviceplanname)
            .WithRegion(defaultlinuxregion)
            .WithExistingResourceGroup(resourcegroupname)
            .WithPricingTier(PricingTier.BasicB1)
            .WithOperatingSystem(OperatingSystem.Linux)
            .Create()

    logmessage "app service plan created."
    output



let public deleteserviceplan(planid:string, logmessage:string -> unit) : unit =
    String.concat valueseparator [
        "deleting app service plan of Id";
        planid
    ]
    |> logmessage
    let azuregateway = createauthenticatedgateway()

    azuregateway.AppServices
        .AppServicePlans
        .DeleteById(planid)

    logmessage "app service plan deleted."
    ()
