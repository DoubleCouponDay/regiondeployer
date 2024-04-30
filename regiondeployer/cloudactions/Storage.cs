using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Linq;
using Microsoft.Azure.Management.AppService.Fluent;
using System.Configuration;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Azure.Management.Storage.Fluent;

namespace RegionDeployer
{
    public static class StorageManager
    {
        public const string DefaultStorageName = "mmstorage";
        public const int MaxStorageNameSize = 24;

        // Helper method to create an authenticated gateway
        private static IAuthenticated CreateAuthenticatedGateway()
        {
            // Assuming method implementation here
            return null;
        }

        // Helper method to create a random name
        private static string CreateRandomName(string baseName, int maxSize)
        {
            // Assuming method implementation here
            return null;
        }

        // Method to create a storage account in a resource group
        private static IStorageAccount CreateStorageAccountInResourceGroup(string storageAccountName, string resourceGroupName, Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            logMessage($"Creating storage account with existing group: {resourceGroupName}");

            var output = gateway.StorageAccounts
                                .Define(storageAccountName)
                                .WithRegion(Region.Default)
                                .WithExistingResourceGroup(resourceGroupName)
                                .WithGeneralPurposeAccountKindV2()
                                .WithOnlyHttpsTraffic()
                                .WithSku(StorageAccountSkuType.Standard_LRS) // LRS is the cheapest redundancy strategy
                                .Create();

            logMessage("Storage account created.");
            return output;
        }

        // Method to create a storage account with a random name
        public static IStorageAccount CreateRandomNamedStorageAccount(string resourceGroupName, Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            var randomName = CreateRandomName(DefaultStorageName, MaxStorageNameSize);
            var nameChecked = gateway.StorageAccounts.CheckNameAvailability(randomName);

            if (nameChecked.IsAvailable.HasValue && nameChecked.IsAvailable.Value)
            {
                return CreateStorageAccountInResourceGroup(randomName, resourceGroupName, logMessage);
            }
            else
            {
                return CreateRandomNamedStorageAccount(resourceGroupName, logMessage);
            }
        }
    }
}
