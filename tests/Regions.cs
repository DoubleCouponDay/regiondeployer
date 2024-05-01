using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class WhenRegionsAreEvaluated
    {
        private readonly ITestOutputHelper _inputLogger;
        private readonly AzureContext _azureContext;

        public WhenRegionsAreEvaluated(ITestOutputHelper inputLogger)
        {
            _inputLogger = inputLogger;
            ConfigAdapter.Initialise();
            _azureContext = new WhenItAuthenticatesToAzure(inputLogger).ItSucceeds();
        }

        private string ListContainsRegion(string questionRegion, AzureRegions.Root[] regionList)
        {
            return regionList.Select(currentRegion =>
                currentRegion.Name.Equals(questionRegion, StringComparison.InvariantCultureIgnoreCase) ? currentRegion.Name : null
            ).FirstOrDefault();
        }

        [Fact]
        public void AllRegionsAreAvailableInSubscription()
        {
            var jsonRegions = AzureRegions.Load(AzureRegionsJsonPath);
            foreach (var location in _azureContext.GetCurrentSubscription().ListLocations())
            {
                var outcome = ListContainsRegion(location.Region.Name, jsonRegions);
                Debug.Assert(outcome == location.Region.Name);
            }
        }
    }
}
