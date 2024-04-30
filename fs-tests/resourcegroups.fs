module regiondeployer.tests.resourcegroup

open Xunit
open regiondeployer.cloudactions.resourcegroup
open Microsoft.Azure.Management.Fluent
open System
open regiondeployer.clouderror
open Xunit.Abstractions
open regiondeployer

///handles disposing the resourcegroup
type public when_it_creates_a_resource_group(inputlogger:ITestOutputHelper) = 
    do
        configadapter.initialise();

    member val private groupname:string = null with get, set
    member private x.logger = inputlogger

    interface IDisposable with member this.Dispose() = deleteresourcegroup(this.groupname, inputlogger.WriteLine)

    [<Fact>]
    member public this.it_succeeds() : string =
        this.groupname <- (createrandomresourcegroup(defaultregion, this.logger.WriteLine))
        this.groupname

    [<Fact>]
    member public this.its_name_is_such_a_length() =
        let newregionname = this.it_succeeds()
        Assert.True(newregionname.Length = maxgroupnamelength)

    [<Fact>]    
    member public this.its_name_contains_default_group_name() =
        let newregionname = this.it_succeeds()
        Assert.True(newregionname.Contains(basegroupname))    

