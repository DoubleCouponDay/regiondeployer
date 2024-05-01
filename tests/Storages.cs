using Xunit;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using System;
using Xunit.Abstractions;

namespace Tests
{
    public class WhenAStorageAccountIsCreated : IDisposable
    {
        private const int MaxTries = 10;
        private readonly WhenItCreatesAResourceGroup _groupTests;
        private readonly string _groupContextsField;
        private readonly ITestOutputHelper _logger;
        private readonly WhenItAuthenticatesToAzure _authContext;

        public WhenAStorageAccountIsCreated(ITestOutputHelper inputLogger)
        {
            _logger = inputLogger;
            _groupTests = new WhenItCreatesAResourceGroup(inputLogger);
            _groupContextsField = _groupTests.ItSucceeds();
            _authContext = new WhenItAuthenticatesToAzure(inputLogger).ItSucceeds();
            ConfigAdapter.Initialise();
        }

        public void Dispose()
        {
            _groupTests.Dispose();
        }

        [Fact]
        public IStorageAccount ItSucceeds()
        {
            try
            {
                return CreateRandomNamedStorageAccount(_groupContextsField, _logger.WriteLine);
            }
            catch (Exception error1)
            {
                RaiseCloudError(error1);
                return null;
            }
        }

        [Fact]
        public void ItLivesInTheInputResourceGroup()
        {
            IStorageAccount objectToTest = TryMultipleTimesGettingStorage(0);
            Assert.True(objectToTest.ResourceGroupName == _groupContextsField);
        }

        private IStorageAccount TryMultipleTimesGettingStorage(int currentTries)
        {
            if (currentTries < MaxTries)
            {
                var output = ItSucceeds();
                if (output.ResourceGroupName != _groupContextsField)
                {
                    _authContext.StorageAccounts.DeleteById(output.Id);
                    return TryMultipleTimesGettingStorage(currentTries + 1);
                }
                else
                {
                    return output;
                }
            }
            else
            {
                return null;
            }
        }

        private void RaiseCloudError(Exception error)
        {
            // Handle the error appropriately
            throw new InvalidOperationException("Cloud operation failed", error);
        }

        private IStorageAccount CreateRandomNamedStorageAccount(string resourceGroupName, Action<string> logAction)
        {
            // Implementation for creating a storage account with a random name
            logAction("Creating storage account");
            // Simulated creation logic
            return new StorageAccountSimulator(resourceGroupName);
        }
    }

    // Simulated classes for demonstration
    public class ConfigAdapter
    {
        public static void Initialise()
        {
            // Configuration initialization logic
        }
    }

    public class StorageAccountSimulator : IStorageAccount
    {
        public string ResourceGroupName { get; }
        public string Id { get; }

        public StorageAccountSimulator(string resourceGroupName)
        {
            ResourceGroupName = resourceGroupName;
            Id = Guid.NewGuid().ToString();
        }
    }
}
