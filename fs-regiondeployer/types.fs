namespace regiondeployer

open FSharp.Data
open System

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