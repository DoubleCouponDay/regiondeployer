module regiondeployer.types

open FSharp.Data
open System
open regiondeployer.constants.independentconstants
open regiondeployer.constants.pathconstants

type public Arguments =
    {
        regionalfarmersrootpath : string
        ContractsFolderPath: string
        NumberOfFarmers : int
    }

//I cant use the json schema here because json provider doesnt remove schema keywords!
type public exchangecontract = JsonProvider<defaultcontractpath>

type public azureregions = JsonProvider<azureregionsjsonpath>

type public ipstack = JsonProvider<placeholderipstackaddress>