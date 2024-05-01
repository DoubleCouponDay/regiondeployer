namespace RegionDeployer;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.AppService.Fluent;
using RegionDeployer;

public static class ListLock
{
    public static readonly object Resource = new object();
}

public class Farmer
{
    private const string BaseFarmerName = "mmregionalfarmer";
    private const int MaxFarmerNameLength = 40;

    public Dictionary<string, string> GetAppSettings()
    {
        var output = new Dictionary<string, string>
        {
            ["FUNCTIONS_EXTENSION_VERSION"] = "~2",
            ["AzureWebJobsSecretStorageType"] = "Files",
            ["WEBSITE_RUN_FROM_PACKAGE"] = "1",
            ["WEBSITE_NODE_DEFAULT_VERSION"] = "8.11.1"
        };
        return output;
    }

    public IEnumerable<IFunctionApp> GetFunctionApps(Action<string> logMessage, ConfigFile config)
    {
        var authenticator = new AzureAuthenticator();
        var gateway = authenticator.CreateAuthenticatedGateway();
        IEnumerable<IFunctionApp> apps;
        lock (ListLock.Resource)
        {
            apps = gateway.AppServices.FunctionApps.List();
        }
        return apps;
    }

    public string GetRandomFarmerName(Action<string> logMessage)
    {
        logMessage("getting random farmer name...");
        var randomName = CreateRandomName(BaseFarmerName, MaxFarmerNameLength);
        var functionApps = GetFunctionApps(logMessage);

        var nameChecked = functionApps.FirstOrDefault(app => app.Name == randomName);

        logMessage($"checking name is unique: {randomName}");

        if (nameChecked == null)
        {
            logMessage("name is unique");
            return randomName;
        }
        else
        {
            logMessage("name is not unique. recursing...");
            return GetRandomFarmerName(logMessage);
        }
    }

    private string ConvertHostNameToIp(string hostname)
    {
        var globalConfig = ProjectCredentials.Get.MatchingCurrentEnvironment();
        var accessKey = globalConfig.IpStackKey;
        var url = $"http://api.ipstack.com/{hostname}?access_key={accessKey}";
        var response = IpStack.Load(url);
        return response.Ip;
    }

    public bool ExternalIpAlreadyExistsInGroup(string hostnameToTest, string resourceGroupName, Action<string> logMessage)
    {
        var questionAppsIp = ConvertHostNameToIp(hostnameToTest);
        logMessage($"checking that farmer's external IP {questionAppsIp} does not already exist in resource group {resourceGroupName}");

        bool groupAndHostNameMatch(IFunctionApp someApp)
        {
            var queryAppsIp = ConvertHostNameToIp(someApp.DefaultHostName);
            return someApp.ResourceGroupName == resourceGroupName && questionAppsIp == queryAppsIp;
        }

        var query = GetFunctionApps(logMessage).Where(groupAndHostNameMatch);

        var existingCount = query.Count();

        if (existingCount == 1)
        {
            logMessage($"farmer's IP {questionAppsIp} is unique.");
            return false;
        }
        else
        {
            logMessage($"farmer's IP {questionAppsIp} already exists {existingCount} times.");
            return true;
        }
    }

    public IFunctionApp CreateFarmer(string appName, string regionName, string existingResourceGroupName, IStorageAccount existingStorageAccount, Action<string> logMessage)
    {
        logMessage($"creating definition of farmer: {appName}, {regionName}");

        var gateway = CreateAuthenticatedGateway();

        var output = gateway.AppServices.FunctionApps
            .Define(appName)
            .WithRegion(regionName)
            .WithExistingResourceGroup(existingResourceGroupName)
            .WithExistingStorageAccount(existingStorageAccount)
            .WithNewConsumptionPlan()
            .WithAppSettings(GetAppSettings())
            .WithHttpsOnly(true)
            .WithContainerLoggingEnabled()
            .WithRemoteDebuggingDisabled()
            .Create();

        logMessage($"definition created: {output.ToString()}");
        output.Start();

        return output;
    }

    public void DeleteSingleFarmer(string farmerId, Action<string> logMessage)
    {
        logMessage($"deleting farmer of id: {farmerId}");
        var gateway = CreateAuthenticatedGateway();
        gateway.AppServices.FunctionApps.DeleteById(farmerId);
        logMessage("farmer deleted.");
    }

    public int DeleteFarmers(string farmerName, Action<string> logMessage)
    {
        int farmersDeleted = 0;
        var functionApps = GetFunctionApps(logMessage);
        foreach (var farmer in functionApps.Where(farmer => farmer.Name == farmerName))
        {
            DeleteSingleFarmer(farmer.Id, logMessage);
            farmersDeleted++;
        }
        return farmersDeleted;
    }
}