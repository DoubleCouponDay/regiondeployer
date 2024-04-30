module regiondeployer.tests.serviceplans

open Xunit
open regiondeployer.cloudactions.buggedlinuxapp
open regiondeployer.cloudactions.serviceplan
open tests
open storage
open Xunit.Abstractions
open System
open Microsoft.Azure.Management.AppService.Fluent
open System
open regiondeployer.clouderror
open regiondeployer

type public when_a_linux_service_plan_is_created(inputlogger:ITestOutputHelper) =
    let storagetestsfield = new when_a_storage_account_is_created(inputlogger)
    let storagecontextsfield = storagetestsfield.it_succeeds()

    do
        configadapter.initialise();

    member public x.storagetests = storagetestsfield
    member public x.storagecontext = storagecontextsfield

    interface IDisposable with member this.Dispose() =
        (storagetestsfield :> IDisposable).Dispose()

    [<Fact>]
    member public this.it_succeeds() : IAppServicePlan =
        try
            let testcontext:IAppServicePlan = createlinuxserviceplan(
                storagetestsfield.groupcontext, 
                inputlogger.WriteLine
            )
            testcontext

        with 
        | error ->
            printclouderror(error, inputlogger.WriteLine)
            raiseclouderror error
            null
        
type public when_a_bugged_linux_app_fix_is_applied(inputlogger:ITestOutputHelper) = 
    let serviceplantests = new when_a_linux_service_plan_is_created(inputlogger)
    let serverplancontext = serviceplantests.it_succeeds()

    let is = true

    interface IDisposable with member this.Dispose() =
        (serviceplantests :> IDisposable).Dispose()

    [<Fact>]
    member public this.it_succeeds() : unit =
        try
            let resourcegroupname = serviceplantests.storagetests.groupcontext
            let storagegroup = serviceplantests.storagecontext
            fixlinuxappbug(resourcegroupname, inputlogger.WriteLine, storagegroup)
            ()

        with 
        | error ->
            printclouderror(error, inputlogger.WriteLine)
            raiseclouderror error
            ()

