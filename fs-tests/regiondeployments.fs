module regiondeployer.tests.regiondeployment

open regiondeployer.deployer
open tests
open Xunit
open regiondeployer.cloudactions.resourcegroup
open regiondeployer.constants.independentconstants
open regiondeployer.types
open System
open Microsoft.Azure.Management.AppService.Fluent
open regiondeployer.cloudactions.azurefunction.farmer
open regiondeployer.clouderror
open Xunit.Abstractions
open regiondeployer.logger
open resourcegroup
open storage
open projectzipper
open farmer
open regiondeployer.constants.personalpaths
open regiondeployer.constants.pathconstants
open regiondeployer

type public when_one_farmer_is_deployed(inputlogger : ITestOutputHelper) =
    let grouptests = new when_it_creates_a_resource_group(inputlogger)
    let groupcontext = grouptests.it_succeeds()

    let storagetests = new when_a_storage_account_is_created(inputlogger)
    let storagecontext = storagetests.it_succeeds()

    do
        configadapter.initialise();
   
    member val private zippercontext = when_it_zips_up_a_project(inputlogger).it_succeeds()
    member x.logger = inputlogger

    interface IDisposable with member this.Dispose() =
        (grouptests :> IDisposable).Dispose()
        (storagetests :> IDisposable).Dispose()

    [<Fact>]
    member public this.it_succeeds() : IFunctionApp = 
        try
            use farmertests = new when_a_farmer_is_created(this.logger)
            let testfarmername = getrandomfarmername(this.logger.WriteLine)

            deployone(
                testfarmername,
                defaultregion,
                this.zippercontext,
                groupcontext,
                storagecontext,
                this.logger.WriteLine
            )
            |> fun result -> match result with
            | (createdresource, deploymentstatus) -> 
                Assert.True((deploymentstatus = true), "region was deployed too")
                createdresource

        with
        | error1 ->
            raiseclouderror error1  
            null

type public when_an_array_of_farmers_are_deployed(inputlogger : ITestOutputHelper) =
    let grouptests = new when_it_creates_a_resource_group(inputlogger)
    let groupcontext = grouptests.it_succeeds()

    let storagetests = new when_a_storage_account_is_created(inputlogger)
    let storagecontext = storagetests.it_succeeds()

    member val private zippercontext = when_it_zips_up_a_project(inputlogger).it_succeeds()
    member private x.logger = inputlogger

    interface IDisposable with member this.Dispose() = (grouptests :> IDisposable).Dispose()

    [<Fact>]
    member public this.it_succeeds() : bool =
        let arguments:Arguments = {
            regionalfarmersrootpath = regionalfarmersroot
            ContractsFolderPath = pathtocontractsfolder
            NumberOfFarmers = 25
        }
        deployarray(
            arguments,
            this.zippercontext,
            true,groupcontext,
            this.logger.WriteLine, 
            storagecontext
        )