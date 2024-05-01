using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MoonMachine.Core;
using MoonMachine.Infrastructure.Entities;
using regiondeployer.nodeadapter;

namespace Tests
{
    public class WhenTheNodeAdapterGetsMarkets
    {
        private string mmexchangespathcontext = regiondeployer.constants.personalpaths.exchangeadaptersroot;

        [Theory]
        [InlineData(coreconstants.defaultexchangename)]
        public IEnumerable<AvailableMarket> ItSucceeds(string exchangename)
        {
            var result = adapter.getmarkets(exchangename, mmexchangespathcontext);
            Assert.True(result != null, "a result was given");
            Assert.True(result.Count() != 0, "some markets were returned");
            return result;
        }

        [Fact]
        public void TheCurrenciesAreNotEmpty()
        {
            var exchangenamecontext = coreconstants.defaultexchangename;
            var adaptercontext = ItSucceeds(exchangenamecontext);

            foreach (var market in adaptercontext)
            {
                Assert.False(String.IsNullOrEmpty(market.HoardedCurrency), "hoarded is not empty");
                Assert.False(String.IsNullOrEmpty(market.PriceCurrency), "price is not empty");
            }
        }
    }
}
