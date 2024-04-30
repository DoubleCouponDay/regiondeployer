module regiondeployer.tests.storage

open Xunit
open regiondeployer.cloudactions.storage
open Microsoft.Azure.Management.Fluent
open regiondeployer.cloudactions.resourcegroup
open System
open Microsoft.Azure.Management.Storage.Fluent
open regiondeployer.randomnamegenerator
open System.Linq
open regiondeployer.clouderror
open Xunit.Abstractions
open resourcegroup
open azureauthentication
open regiondeployer

type public when_a_storage_account_is_created(inputlogger:ITestOutputHelper) =    
    let maxtries = 10 //total edge case. there will never be 10 but there could be! /tableflip
    let grouptests = new when_it_creates_a_resource_group(inputlogger)
    let groupcontextsfield : string = grouptests.it_succeeds()     

    do
        configadapter.initialise();

    member val private authcontext = (new when_it_authenticates_to_azure(inputlogger)).it_succeeds()
    member private x.logger = inputlogger

    member public x.groupcontext = groupcontextsfield

    interface IDisposable with member this.Dispose() = 
        (grouptests :> IDisposable).Dispose()

    [<Fact>]
    ///returns a storage account in an isolated resourcegroup. if it was part of an existing group this would cause exceptions during test disposable.
    member public this.it_succeeds() : IStorageAccount =   
        try
            createrandomnamedstorageaccount(groupcontextsfield, this.logger.WriteLine)

        with
        | error1 -> 
            raiseclouderror error1  
            null

    [<Fact>]
    member public this.it_lives_in_the_input_resourcegroup() =
        let rec trymultipletimesgettingstorage (currenttries:int) : IStorageAccount =
            if currenttries < maxtries then
                let output = this.it_succeeds()

                if output.ResourceGroupName <> groupcontextsfield then //just in case I ran a test while there were existing farmers
                    (this.authcontext
                        .StorageAccounts
                        .DeleteById output.Id)

                    trymultipletimesgettingstorage (currenttries + 1)

                else
                    output

            else
                null

        try
            let objecttotest = trymultipletimesgettingstorage 0

            Assert.True(
                objecttotest.ResourceGroupName = groupcontextsfield
            )

        with
        | error1 -> 
            raiseclouderror error1  


