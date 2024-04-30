module regiondeployer.tests.nodeadapting

open System
open System.Collections.Generic
open System.Linq
open MoonMachine.Core
open MoonMachine.Infrastructure.Entities
open Xunit
open regiondeployer.nodeadapter
open System

type public when_the_node_adapter_gets_markets() =
    let mmexchangespathcontext = regiondeployer.constants.personalpaths.exchangeadaptersroot
    
    [<Theory>]
    [<InlineData(coreconstants.defaultexchangename)>]
    member public x.it_succeeds(exchangename: string) : IEnumerable<AvailableMarket> = 
        let result = adapter.getmarkets(exchangename, mmexchangespathcontext)
        Assert.True(result <> null, "a result was given")
        Assert.True(result.Count() <> 0, "some markets were returned")
        result

    [<Fact>]
    member public x.the_currencies_are_not_empty(): unit =
        let exchangenamecontext = coreconstants.defaultexchangename
        let adaptercontext = x.it_succeeds(exchangenamecontext)

        for market in adaptercontext do
            Assert.False(String.IsNullOrEmpty(market.HoardedCurrency), "hoarded is not empty")
            Assert.False(String.IsNullOrEmpty(market.PriceCurrency), "price is not empty")
        
