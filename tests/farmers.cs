using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using Xunit;
using Xunit.Abstractions;
using System;

namespace Tests
{
    public class WhenAFarmerIsCreated : IDisposable
    {
        private ITestOutputHelper _logger;
        private WhenAStorageAccountIsCreated _storageTests;
        private StorageContext _storageContext;
        private WhenItCreatesAResourceGroup _groupTests;
        private GroupContext _groupContext;
        private AzureContext _azureContext;

        public WhenAFarmerIsCreated(ITestOutputHelper logger)
        {
            _logger = logger;
            _storageTests = new WhenAStorageAccountIsCreated(logger);
            _storageContext = _storageTests.ItSucceeds();

            _groupTests = new WhenItCreatesAResourceGroup(logger);
            _groupContext = _groupTests.ItSucceeds();

            ConfigAdapter.Initialise();

            _azureContext = new WhenItAuthenticatesToAzure(logger).ItSucceeds();
        }

        public void Dispose()
        {
            _storageTests.Dispose();
            _groupTests.Dispose();
        }

        [Theory]
        [InlineData("1" + BaseFarmerName, DefaultRegion)]
        public IFunctionApp ItSucceeds(string farmerName, string regionName)
        {
            try
            {
                return CreateFarmer(
                    farmerName,
                    regionName,
                    _groupContext,
                    _storageContext,
                    _logger.WriteLine
                );
            }
            catch (Exception error1)
            {
                RaiseCloudError(error1);
                return null;
            }
        }

        [Fact]
        public void TheExternalIpOfItAndAnotherInTheSameRegionDontConflict()
        {
            try
            {
                var farmer1 = ItSucceeds(GetRandomFarmerName(_logger.WriteLine), DefaultRegion);
                var farmer2 = ItSucceeds(GetRandomFarmerName(_logger.WriteLine), DefaultRegion);

                Assert.False(
                    ExternalIpAlreadyExistsInGroup(
                        farmer1.DefaultHostName,
                        _groupContext,
                        _logger.WriteLine
                    )
                );

                Assert.False(
                    ExternalIpAlreadyExistsInGroup(
                        farmer2.DefaultHostName,
                        _groupContext,
                        _logger.WriteLine
                    )
                );
            }
            catch (Exception error1)
            {
                RaiseCloudError(error1);
            }
        }

        [Fact]
        public void ItWasAssignedToThisRegion()
        {
            try
            {
                var farmer = ItSucceeds(GetRandomFarmerName(_logger.WriteLine), DefaultRegion);
                Assert.True(farmer.RegionName == DefaultRegion);
            }
            catch (Exception error1)
            {
                RaiseCloudError(error1);
            }
        }
    }
}