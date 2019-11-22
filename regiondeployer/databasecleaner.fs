module regiondeployer.databasecleaner

open regiondeployer.types
open MoonMachine.Infrastructure
open MoonMachine.Infrastructure.Identity
open MoonMachine.Infrastructure.Entities
open System.Configuration
open System.IO
open System.Linq
open System
open FSharp.Data
open System.Data.Entity
open nodeadapter.adapter
open Microsoft.Azure.Management.AppService.Fluent
open regiondeployer.constants.independentconstants
open MoonMachine.Infrastructure.models
open System.Collections.Generic
open System.Data.Entity.Validation
open System.Linq
open regiondeployer.constants.pathconstants
open regiondeployer.azureauthenticator
open regiondeployer.connectionstringoverrider

let private removeregionalfarmerrecords (logmessage:string -> unit, database:ApplicationDbContext) : unit =
    logmessage "removing previous regional farmer records from database..."

    database.RegionalFarmers
        .Where(
            fun farmer -> true
        )
        .AsEnumerable()
    |> database.RegionalFarmers.RemoveRange  
    |> ignore
    database.SaveChanges()
    logmessage "previous farmer records removed."

let private removeapicontractrecords (logmessage:string -> unit, database:ApplicationDbContext) : unit =
    logmessage "removing previous api contract records..."

    database.authenticationformats
        .AsEnumerable()
    |> database.authenticationformats.RemoveRange
    |> ignore

    for exchange in database.AvailableExchanges do
        exchange.iscurrent <- false

    for market in database.AvailableMarkets do
        market.iscurrent <- false //cant remove since a bunch of historic records depend on markets and exchanges existing in relationships

    database.SaveChanges()
    logmessage "previous api contract records removed."
    ()

let private removescriptinglanguages(logmessage: string -> unit, database:ApplicationDbContext) : unit =
    logmessage "removing previous scripting language records..."

    for currentlang in database.Languages do
        currentlang.iscurrent <- false

    database.SaveChanges()
    logmessage "previous scripting language records removed."
    ()

type public databasecleaner() =

    ///connection string is optional
    static member public removeinitialstate(logmessage: string -> unit, ?connectionstring:string) : unit =
        use database = overrideconnectionstring(connectionstring)
        removescriptinglanguages(logmessage, database)
        removeapicontractrecords(logmessage, database)
        removeregionalfarmerrecords(logmessage, database)
        ()

