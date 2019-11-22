module regiondeployer.tests.farmer

open Microsoft.Azure.Management.Fluent
open regiondeployer.cloudactions.resourcegroup
open Xunit
open regiondeployer.constants.independentconstants
open regiondeployer.databaseseeder
open System
open regiondeployer.clouderror
open Microsoft.Azure.Management.AppService.Fluent
open Xunit.Abstractions
open tests
open storage
open resourcegroup
open azureauthentication
open regiondeployer.cloudactions.azurefunction.farmer
open regiondeployer

type public when_a_farmer_is_created(inputlogger:ITestOutputHelper) = 
    let storagetests = new when_a_storage_account_is_created(inputlogger)
    let storagecontext = storagetests.it_succeeds()

    let grouptests = new when_it_creates_a_resource_group(inputlogger)
    let groupcontext = grouptests.it_succeeds()

    do
        configadapter.initialise();
    
    member private x.logger = inputlogger

    member val private azurecontext = (new when_it_authenticates_to_azure(inputlogger)).it_succeeds()

    interface IDisposable with member this.Dispose() = 
        (storagetests :> IDisposable).Dispose() 
        (grouptests :> IDisposable).Dispose()

    [<Theory>]
    [<InlineData(("1" + basefarmername), defaultregion)>] //this can conflict with same name farmer if this test runs in parallel with deployarray test.
    member public this.it_succeeds(farmername:string, regionname:string) : IFunctionApp =
        try
            createfarmer(
                farmername,
                regionname,
                groupcontext,
                storagecontext,
                this.logger.WriteLine
            )
        //Im using catch to dispose instead of finally because other tests may be dependend on the internal state if this method succeeds.
        with 
        | error1 ->
            raiseclouderror error1
            null

    [<Fact>]    
    member public this.the_external_ip_of_it_and_another_in_the_same_region_dont_conflict() =
        try
            let farmer1 = this.it_succeeds(getrandomfarmername(this.logger.WriteLine), defaultregion)        
            let farmer2 = this.it_succeeds(getrandomfarmername(this.logger.WriteLine), defaultregion)

            externalipalreadyexistsingroup(
                farmer1.DefaultHostName,
                groupcontext,
                this.logger.WriteLine
            )
            |> Assert.False

            externalipalreadyexistsingroup(
                farmer2.DefaultHostName,
                groupcontext,
                this.logger.WriteLine
            )
            |> Assert.False

        with
        | error1 ->
            raiseclouderror error1            

    [<Fact>]
    member public this.it_was_assigned_to_this_region() =
        try
            let farmer = this.it_succeeds(getrandomfarmername(this.logger.WriteLine), defaultregion)
            Assert.True(farmer.RegionName = defaultregion)

        with
        | error1 ->
            raiseclouderror error1
    



