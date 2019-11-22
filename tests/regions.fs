module regiondeployer.tests.regions

open System.Diagnostics
open regiondeployer.types
open regiondeployer.constants.independentconstants
open regiondeployer.constants.pathconstants
open System.Linq
open System
open Microsoft.Azure.Management.ResourceManager.Fluent
open FSharp.Data.Runtime
open Xunit
open Xunit.Abstractions
open azureauthentication
open regiondeployer

type public when_regions_are_evaluated(inputlogger:ITestOutputHelper) =
    do
        configadapter.initialise();

    member val public azurecontext = (new when_it_authenticates_to_azure(inputlogger)).it_succeeds()

    ///returns the matching region, else null
    member private this.listcontainsregion (questionregion:string) (regionlist:azureregions.Root []) : string =
        regionlist.Select(
            fun currentregion ->
                if currentregion.Name.Equals(questionregion, StringComparison.InvariantCultureIgnoreCase) then
                    currentregion.Name

                else
                    null
        ).FirstOrDefault()

    [<Fact>]
    member public this.all_regions_are_available_in_subscription() =
        this.azurecontext
            .GetCurrentSubscription()
            .ListLocations()
            .Select(
                fun (location:ILocation) ->
                    let jsonregions = azureregions.Load(azureregionsjsonpath)
                    this.listcontainsregion location.Region.Name jsonregions
                    |> fun outcome -> 
                        Debug.Assert((outcome = location.Region.Name))
            )        
