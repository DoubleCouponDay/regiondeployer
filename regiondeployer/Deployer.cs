using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Management.Storage.Fluent;
using Newtonsoft.Json;

namespace regiondeployer
{
    class RegionDeployer
    {
        private static int concurrentDeploymentsLimit = AzureRegions.Load(azureRegionsJsonPath).Length;

        private static HttpResponseMessage PostDeploymentRequest(string appName, string zipFilePath, string username, string password, Action<string> logMessage)
        {
            using (var http = new HttpClient())
            {
                logMessage("making post request to apps kudu interface...");
                var filesBytes = File.ReadAllBytes(zipFilePath);
                var httpContent = new ByteArrayContent(filesBytes);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var basicAuthPair = HttpUtilities.CreateBasicAuthHeader(username, password);

                var address = $"https://{appName}.scm.azurewebsites.net/api/zipdeploy";

                var requestMessage = HttpUtilities.CreateRequestMessage(address, HttpMethod.Post, httpContent, basicAuthPair);

                var output = http.SendAsync(requestMessage).Result;

                requestMessage.Dispose();

                logMessage("response received.");
                return output;
            }
        }

        public static (IFunctionApp, bool) DeployOne(string appName, string regionName, string zipFilePath, string resourceGroupName, IStorageAccount storageAccount, Action<string> logMessage)
        {
            var farmer = CreateFarmer(appName, regionName, resourceGroupName, storageAccount, logMessage);
            farmer.Stop(); // Prevent file copy locking issues.

            logMessage($"deploying regional farmer...{appName}{regionName}");

            var publishingProfile = farmer.GetPublishingProfile();
            var username = publishingProfile.GitUsername;
            var password = publishingProfile.GitPassword;

            var response = PostDeploymentRequest(farmer.Name, zipFilePath, username, password, logMessage);

            if (!response.IsSuccessStatusCode)
            {
                logMessage($"deployment failed for farmer {farmer.Name} {response.StatusCode} {response.ReasonPhrase}");
            }
            else
            {
                logMessage($"deployed to regional farmer. {regionName}");
            }
            farmer.Start();
            response.Dispose();
            return (farmer, response.IsSuccessStatusCode);
        }

        public static bool DeployArray(Arguments inputs, string zipFilePath, bool failOnError, string resourceGroupName, Action<string> logMessage, IStorageAccount storageAccount)
        {
            logMessage($"deploying the array of regional farmers...{inputs.NumberOfFarmers} long.");

            var regions = AzureRegions.Load(azureRegionsJsonPath);

            int deploymentIndex = 0;
            int successfulDeployments = 0;

            while (deploymentIndex < inputs.NumberOfFarmers)
            {
                int slotSize = Math.Min(inputs.NumberOfFarmers - deploymentIndex, concurrentDeploymentsLimit);
                int slotEndIndex = deploymentIndex + slotSize;

                logMessage($"deploying farmers {deploymentIndex} up to {slotEndIndex}");

                Parallel.For(deploymentIndex, slotEndIndex, i =>
                {
                    try
                    {
                        logMessage($"deploying farmer {i}");

                        var currentRegion = i < regions.Length ? regions[i].Name : defaultRegion;
                        var randomFarmerName = GetRandomFarmerName(logMessage);

                        var (createdResource, deploymentStatus) = DeployOne(randomFarmerName, currentRegion, zipFilePath, resourceGroupName, storageAccount, logMessage);

                        if (deploymentStatus)
                        {
                            var masterKey = createdResource.GetMasterKey();
                            DatabaseSeeder.SeedNewRegionalFarmer(createdResource, masterKey, logMessage);
                            logMessage($"region farmer {i} deployed. {currentRegion}");
                            successfulDeployments++;
                        }
                    }
                    catch (Exception error)
                    {
                        if (failOnError)
                        {
                            PrintCloudError(error, logMessage);
                            throw;
                        }
                        else
                        {
                            PrintCloudError(error, logMessage);
                        }
                    }
                });

                deploymentIndex = slotEndIndex;
            }

            logMessage($"array deployed. there were {successfulDeployments}/{inputs.NumberOfFarmers} successful deployments.");
            return true;
        }
    }
}
