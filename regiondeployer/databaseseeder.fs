module regiondeployer.databaseseeder

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
open regiondeployer.contractfiler
open regiondeployer.connectionstringoverrider

let private createnewauthenticationformat (database:ApplicationDbContext, exchange:AvailableExchange, contract:exchangecontract.Root, logmessage:string -> unit) : authenticationformat =
    String.concat valueseparator ["creating new authentication format for exchange"; exchange.ExchangeName]
    |> logmessage
    
    use writer = new StringWriter()
    let newformat = new authenticationformat()

    newformat.exchange <- exchange

    contract.Info.Authentication.Content.JsonValue.WriteTo (writer, JsonSaveOptions.None)
    newformat.content <- writer.ToString()
    writer.Flush()

    contract.Info.Authentication.Headers.JsonValue.WriteTo (writer, JsonSaveOptions.None)
    newformat.headers <- writer.ToString()
    writer.Flush()

    contract.Info.Authentication.Parameters.JsonValue.WriteTo (writer, JsonSaveOptions.None)
    newformat.parameters <- writer.ToString()

    database.authenticationformats.Add(newformat)
    database.SaveChanges()

    logmessage "authentication format created."
    newformat

///returns the seeded exchange
let private seedavailableexchange (database:ApplicationDbContext, contract:exchangecontract.Root, logmessage:string -> unit) : AvailableExchange = 
    String.concat valueseparator ["seeding available exchange"; contract.Info.Contractname]
    |> logmessage

    let exchangeduplicate = 
        database.AvailableExchanges.ToList().Where( //linq cannot convert jsonprovider.root to a query so I must execute the query in process.
            fun exchangesearch -> exchangesearch.ExchangeName = contract.Info.Contractname
        ).FirstOrDefault()

    if exchangeduplicate = null then
        let output = new AvailableExchange()
        output.DaysAvailable <- contract.Info.Frequencies.Days
        output.HoursAvailable <- contract.Info.Frequencies.Hours
        output.MonthAvailable <- contract.Info.Frequencies.Months
        output.WeekAvailable <- contract.Info.Frequencies.Weeks

        output.ExchangeName <- contract.Info.Contractname
        output.ratelimitmilliseconds <- contract.Info.Ratelimit  
        output.iscurrent <- true
        let keynotfilled = database.AvailableExchanges.Add(output)
        logmessage "new availableexchange added."
        database.SaveChanges() 
        output

    else
        exchangeduplicate.DaysAvailable <- contract.Info.Frequencies.Days
        exchangeduplicate.HoursAvailable <- contract.Info.Frequencies.Hours
        exchangeduplicate.MonthAvailable <- contract.Info.Frequencies.Months
        exchangeduplicate.WeekAvailable <- contract.Info.Frequencies.Weeks

        exchangeduplicate.ExchangeName <- contract.Info.Contractname
        exchangeduplicate.ratelimitmilliseconds <- contract.Info.Ratelimit  
        exchangeduplicate.iscurrent <- true

        logmessage "existing availabledxchange updated."
        database.SaveChanges() //key is not filled until you save the entity to db
        exchangeduplicate

let private seedavailablemarkets (database:ApplicationDbContext, marketsexchange:AvailableExchange, logmessage:string -> unit) : unit =
    let markets = getmarkets (marketsexchange.ExchangeName, pathtoexchangeadaptersindexfile)
    
    for currentinputmarket in markets do
        String.concat valueseparator ["seeding available markets"; currentinputmarket.HoardedCurrency; currentinputmarket.PriceCurrency]
        |>logmessage

        let marketduplicate = 
            database.AvailableMarkets
                .Where(
                    fun currentdbmarket ->
                        currentdbmarket.ExchangeId = marketsexchange.Id &&
                        currentdbmarket.Exchange.ExchangeName = marketsexchange.ExchangeName &&
                        currentdbmarket.HoardedCurrency = currentinputmarket.HoardedCurrency &&
                        currentdbmarket.PriceCurrency = currentinputmarket.PriceCurrency                        
                ).FirstOrDefault()

        if marketduplicate = null then
            //add new market
            currentinputmarket.ExchangeId <- marketsexchange.Id    
            currentinputmarket.iscurrent <- true
            database.AvailableMarkets.Add currentinputmarket  //currency codes already added
            database.SaveChanges()
            logmessage "added new market."
            ()

        else
            //fill existing market
            marketduplicate.iscurrent <- true
            marketduplicate.MinimumPrice <- currentinputmarket.MinimumPrice
            marketduplicate.MinimumVolume <- currentinputmarket.MinimumVolume
            database.SaveChanges()
            logmessage "filled existing market."
            ()

    logmessage "seeded available markets."
    ()

let private seedwithcontracts (contractsfolderpath:string, logmessage:string -> unit, database:ApplicationDbContext) : unit =
    String.concat valueseparator ["seeding the database with the location's api contracts"; contractsfolderpath]
    |> logmessage 
    let contracts = getvalidcontractfiles(contractsfolderpath, logmessage)

    for contract in contracts do
        let exchangewithid = seedavailableexchange(database, contract, logmessage)
        let newformat = createnewauthenticationformat(database, exchangewithid, contract, logmessage)
        seedavailablemarkets(database, exchangewithid, logmessage)
        |> ignore

    database.SaveChanges()
    logmessage "seeded the database with api contracts."
    ()

let private seedscriptlanguages(logmessage:string -> unit, database:ApplicationDbContext) : IEnumerable<ScriptingLanguage> =
    logmessage("seeding script languages to database...")
    let languagenames = Languages.get.languagenames()

    let encapsulatedlanguages : IEnumerable<ScriptingLanguage> = 
        languagenames.Select(
            fun currentlang -> 
                let newlanguage = new ScriptingLanguage()
                newlanguage.LanguageName <- currentlang
                newlanguage.iscurrent <- true
                newlanguage
        )

    for currentencaplang in encapsulatedlanguages do
        let matchinglang = 
            database.Languages.Where(
                fun currentdblang ->
                    currentdblang.LanguageName = currentencaplang.LanguageName
            ).FirstOrDefault()
            
        if matchinglang = null then
            database.Languages
                .Add(currentencaplang)
            |> ignore

    database.SaveChanges()
    logmessage("script languages added.")
    encapsulatedlanguages

type public databaseseeder() = 
    ///connection string is optional
    static member public seednewregionalfarmer(farmer:IFunctionApp, masterkey:string, logmessage:string -> unit, ?connectionstring:string) : unit =
        String.concat valueseparator ["seeding new farmer info"; farmer.Name]
        |> logmessage 

        use database = overrideconnectionstring(connectionstring)

        let newinfomodel = new RegionalFarmer()
        newinfomodel.Key <- masterkey
        newinfomodel.Region <- farmer.RegionName
        newinfomodel.Hook <- "https://" + farmer.DefaultHostName    
        database.RegionalFarmers.Add(newinfomodel)
        logmessage "seeded new farmer info."
        |> ignore

        database.SaveChanges()
        ()

    static member public addinitialstate(contractfolderspath:string, logmessage:string -> unit, ?connectionstring:string) : IEnumerable<ScriptingLanguage> =
        use database = overrideconnectionstring(connectionstring)
        seedwithcontracts(contractfolderspath, logmessage, database)
        seedscriptlanguages(logmessage, database)