module regiondeployer.nodeadapter.adapter

open EdgeJs
open System.IO
open MoonMachine.Infrastructure.models
open FSharp.Data
open FSharp.Interop.Dynamic
open System.Dynamic
open System.Collections.Generic
open MoonMachine.Infrastructure.Entities
open System
open MoonMachine.Infrastructure.Identity
open serversidearchitecture.common.types
open serversidearchitecture.common.adaptingconstants

let private executenoderuntimejavascriptfile (evalstring:string) (arguments:obj) : obj =
    (Edge.Func(evalstring)
    .Invoke(arguments)
    .Result)

let private convertgenericresulttodouble (result:obj) : double =
    if result :? int then
        let conversionwip = result :?> int
        double(conversionwip)

    else if result :? double then
        result :?> double

    else if result :? float then
        let conversionwip = result :?> float
        double(conversionwip)

    else
        failwith "returned type is not a number?"
        
///markettickers and availableexchanges are not linked
let public getmarkets (inputexchangename:string, pathtommexchanges:string) : seq<AvailableMarket> =
    let evalstring = String.concat "" [
            "return require('";
            pathtommexchanges;
            "').";
            actions.getmarketsaction;
        ]

    let input = new mmexchangesinput(inputexchangename, null, null, null)

    executenoderuntimejavascriptfile evalstring input
    |> fun result -> 
        let dynamiccollectionwip = result :?> Object[]
        let output = new List<AvailableMarket>()

        seq { for item in dynamiccollectionwip do
            let dynamicwip = item :?> ExpandoObject
            let currentoutputbuild = new AvailableMarket()

            //UNSAFE
            currentoutputbuild.MinimumVolume <- convertgenericresulttodouble dynamicwip?MinimumVolume
            currentoutputbuild.MinimumPrice <- convertgenericresulttodouble dynamicwip?MinimumPrice
            currentoutputbuild.HoardedCurrency <- dynamicwip?HoardedCurrency
            currentoutputbuild.PriceCurrency <- dynamicwip?PriceCurrency
            yield currentoutputbuild
        }
