module regiondeployer.tests.databaseseeding

open regiondeployer.databaseseeder
open regiondeployer.databasecleaner
open Xunit
open regiondeployer.constants.independentconstants
open regiondeployer.constants.pathconstants
open regiondeployer.cloudactions.resourcegroup
open Microsoft.Azure.Management.AppService.Fluent
open System.IO
open System.Linq
open regiondeployer.types
open MoonMachine.Infrastructure.Identity
open MoonMachine.Infrastructure.Entities
open System
open regiondeployer.clouderror
open System.Collections.Generic
open MoonMachine.Infrastructure.models
open System.Configuration
open System.Diagnostics
open Xunit.Abstractions
open FSharp.Data
open regiondeployer.tests.farmer
open regiondeployer.cloudactions.azurefunction.farmer
open regiondeployer.contractfiler
open MoonMachine.credentials
open regiondeployer.connectionstringoverrider
open regiondeployer
open regiondeployer.constants
open regiondeployer.nodeadapter
open regiondeployer.tests.nodeadapting

///handles removing deploymentstate from the database
type public when_it_seeds_the_database(inputlogger:ITestOutputHelper) =
    let farmertests = new when_a_farmer_is_created(inputlogger)
    let farmercontext = farmertests.it_succeeds(getrandomfarmername(inputlogger.WriteLine), defaultregion)
    let debugconfig = ProjectCredentials.get.GetDebugCredentials()

    do
        configadapter.initialise();

    member private x.logger = inputlogger

    member val private adaptertests = when_the_node_adapter_gets_markets()
        with get

    interface IDisposable with member this.Dispose() = 
        (farmertests :> IDisposable).Dispose()

    [<Fact>]
    ///returns the functionapp seeded and the contract folders path
    member public this.it_succeeds() : (IFunctionApp * string * IEnumerable<ScriptingLanguage>) =
        
        databasecleaner.removeinitialstate(this.logger.WriteLine, debugconfig.ConnectionString)

        databaseseeder.seednewregionalfarmer(
            farmercontext, farmercontext.GetMasterKey(), this.logger.WriteLine, debugconfig.ConnectionString)

        let languagecontext = databaseseeder.addinitialstate(pathtocontractsfolder, this.logger.WriteLine, debugconfig.ConnectionString)

        (farmercontext, pathtocontractsfolder, languagecontext)

    member private this.testdatabasewithcontractinfo(
        contract:exchangecontract.Root, 
        matchingexchange:AvailableExchange) : unit =

        Assert.True (contract.Info.Frequencies.Days = matchingexchange.DaysAvailable)
        Assert.True (contract.Info.Frequencies.Hours = matchingexchange.HoursAvailable)
        Assert.True (contract.Info.Frequencies.Weeks = matchingexchange.WeekAvailable)
        Assert.True (contract.Info.Frequencies.Months = matchingexchange.MonthAvailable)
        Assert.True (contract.Info.Ratelimit = matchingexchange.ratelimitmilliseconds)

    member private this.testdatabasewithconsumerinfo(dbcontext:ApplicationDbContext, inputmarkets:seq<AvailableMarket>, currentexchange:AvailableExchange) : unit =

        for inputmarket in inputmarkets do
            let currentcomparisonentity = 
                dbcontext.AvailableMarkets.Where(fun market ->
                    market.HoardedCurrency = inputmarket.HoardedCurrency &&
                    market.PriceCurrency = inputmarket.PriceCurrency &&
                    market.MinimumVolume = inputmarket.MinimumVolume &&
                    market.MinimumPrice = inputmarket.MinimumPrice
                ).FirstOrDefault()

            let message = 
                String.concat valueseparator [currentexchange.ExchangeName; inputmarket.HoardedCurrency; inputmarket.PriceCurrency;
                "can be found in the database."]
            Debug.Assert (currentcomparisonentity <> null, message)
            ()

        ()

    member private this.testdatabasewithscriptlanguageinfo(testlanguages:IEnumerable<ScriptingLanguage>) : unit =
        let languagenames = Enum.GetNames(typeof<Languages>)

        for langname in languagenames do            
            testlanguages.Where(
                fun currentlang -> currentlang.LanguageName = langname
            ).First()
            |> ignore

    member private this.testdatabasewithregionfarmerinfo(testfarmer:IFunctionApp, databasefarmer:RegionalFarmer) : unit =
        Assert.True(testfarmer.DefaultHostName = databasefarmer.Hook)
        Assert.True(testfarmer.Key = databasefarmer.Key)
        Assert.True(testfarmer.RegionName = databasefarmer.Region)

    member private this.testdatabasewithauthenticationformatinfo(formatcontainer:exchangecontract.Root, databaseformat:authenticationformat) =
        let format = formatcontainer.Info.Authentication
        use stringwriter = new StringWriter()
        format.Content.JsonValue.WriteTo(stringwriter, JsonSaveOptions.None)
        Assert.True(stringwriter.ToString() = databaseformat.content)
        stringwriter.Flush()

        format.Headers.JsonValue.WriteTo(stringwriter, JsonSaveOptions.None)
        Assert.True(stringwriter.ToString() = databaseformat.headers)
        stringwriter.Flush()

        format.Parameters.JsonValue.WriteTo(stringwriter, JsonSaveOptions.None)
        Assert.True(stringwriter.ToString() = databaseformat.parameters)
        stringwriter.Flush()
        

    [<Fact>]
    member public this.output_values_match_input() = //cant use failwithclouderror here because the stack trace could not be found!
        let seededresult = this.it_succeeds()
        
        match seededresult with
        | (appdeployed, contractspath, languagesseeded) ->
            let connectionoption = Some debugconfig.ConnectionString
            use dbcontext = overrideconnectionstring(connectionoption)
            
            getvalidcontractfiles(contractspath, this.logger.WriteLine)
            |> Seq.iter (fun contract -> 
                let matchingexchange = 
                    dbcontext.AvailableExchanges
                        .ToList() //protects against invalid operation error
                        .Where(fun exchange ->
                            contract.Info
                                .Contractname = exchange.ExchangeName
                    ).First()

                this.testdatabasewithcontractinfo(contract, matchingexchange)
                let adaptercontext = this.adaptertests.it_succeeds(contract.Info.Contractname)
                this.testdatabasewithconsumerinfo(dbcontext, adaptercontext, matchingexchange)

                let formatcontext = 
                    (dbcontext.authenticationformats
                        .Where(
                            fun currentformat -> currentformat.exchange.Id = matchingexchange.Id
                    )).First()
                        
                this.testdatabasewithauthenticationformatinfo(contract, formatcontext)

                let matchingregionalfarmer =
                    dbcontext.RegionalFarmers
                        .Where(
                            fun farmer -> farmer.Key = appdeployed.Key
                        )
                        .First()

                this.testdatabasewithregionfarmerinfo(appdeployed, matchingregionalfarmer)

                this.testdatabasewithscriptlanguageinfo(languagesseeded)
                ()
            )
        ()
            
        