using Xunit;
using System;
using Microsoft.Azure.Management.AppService.Fluent;
using Xunit.Abstractions;
using System;

namespace Tests
{
    public class RegionDeployment
    {
        private readonly ITestOutputHelper _logger;
        private readonly WhenItCreatesAResourceGroup _groupTests;
        private readonly ResourceGroupContext _groupContext;
        private readonly WhenAStorageAccountIsCreated _storageTests;
        private readonly StorageContext _storageContext;
        private readonly ZipperContext _zipperContext;

        public RegionDeployment(ITestOutputHelper logger)
        {
            _logger = logger;
            _groupTests = new WhenItCreatesAResourceGroup(logger);
            _groupContext = _groupTests.ItSucceeds();
            _storageTests = new WhenAStorageAccountIsCreated(logger);
            _storageContext = _storageTests.ItSucceeds();
            _zipperContext = new WhenItZipsUpAProject(logger).ItSucceeds();
            ConfigAdapter.Initialize();
        }

        public void Dispose()
        {
            _groupTests.Dispose();
            _storageTests.Dispose();
        }

        [Fact]
        public IFunctionApp ItSucceeds()
        {
            try
            {
                using var farmerTests = new WhenAFarmerIsCreated(_logger);
                var testFarmerName = GetRandomFarmerName(_logger.WriteLine);

                var result = DeployOne(
                    testFarmerName,
                    DefaultRegion,
                    _zipperContext,
                    _groupContext,
                    _storageContext,
                    _logger.WriteLine
                );

                if (result.DeploymentStatus)
                {
                    Assert.True(result.DeploymentStatus, "Region was deployed too");
                    return result.CreatedResource;
                }
                else
                {
                    throw new Exception("Deployment failed");
                }
            }
            catch (Exception error)
            {
                throw new CloudError(error);
            }
        }

        // Additional methods and classes used in the F# file would also need to be defined here.
    }
}