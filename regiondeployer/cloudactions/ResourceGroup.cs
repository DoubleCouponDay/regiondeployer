using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RegionDeployer
{
    public class ResourceGroup
    {
        private const int MaxGroupNameLength = 90;
        private const string BaseGroupName = "mmfarmergroup";
        private const string DefaultRegion = "westus2";

        private bool GroupNameIsAvailable(string groupName, Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            bool isAvailable = !gateway.ResourceGroups.Contain(groupName);
            return isAvailable;
        }

        public void DeleteResourceGroup(string resourceGroupName, Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            logMessage($"Deleting group: {resourceGroupName}");

            gateway.ResourceGroups.DeleteByName(resourceGroupName);
            logMessage("Group deleted.");
        }

        private string GetBaseEnvironmentName()
        {
            var baseEnvironmentName = ProjectCredentials.GetMatchingCurrentEnvironment().CurrentEnvironment;
            string name = BaseGroupName + Enum.GetName(typeof(Environment), baseEnvironmentName);
            return name;
        }

        public void DeleteFarmerGroupsMatchingCurrentEnvironmentMode(Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            string name = GetBaseEnvironmentName();

            var matchingGroups = gateway.ResourceGroups
                .List()
                .Where(group => group.Name.ToLower().Contains(name))
                .ToList();

            logMessage($"Deleting old farmer groups with names containing: {name}");

            int groupsDeleted = 1;
            Parallel.ForEach(matchingGroups, (group) =>
            {
                DeleteResourceGroup(group.Name, logMessage);
                Interlocked.Increment(ref groupsDeleted);
            });

            while (groupsDeleted < matchingGroups.Count)
            {
                logMessage("Waiting for parallel delete operations to finish...");
                Thread.Sleep(TimeSpan.FromSeconds(1.0));
            }

            logMessage("Groups deleted.");
        }

        private string GetAvailableRandomName(Action<string> logMessage)
        {
            string basename = GetBaseEnvironmentName();
            string output = CreateRandomName(basename, MaxGroupNameLength);

            while (!GroupNameIsAvailable(output, logMessage))
            {
                output = CreateRandomName(basename, MaxGroupNameLength);
            }

            return output;
        }

        public string CreateRandomResourceGroup(string regionName, Action<string> logMessage)
        {
            var gateway = CreateAuthenticatedGateway();
            string randomGroupName = GetAvailableRandomName(logMessage);

            logMessage($"Creating resource group: {randomGroupName}");

            var resourceGroup = gateway.ResourceGroups
                .Define(randomGroupName)
                .WithRegion(regionName)
                .Create();

            logMessage("Resource group created.");
            return resourceGroup.Name;
        }

        private IAuthenticated CreateAuthenticatedGateway()
        {
            // Assuming this method creates and returns an authenticated gateway to Azure management services.
            return null;
        }

        private string CreateRandomName(string baseName, int maxLength)
        {
            // Assuming this method generates a random name based on the baseName and ensures it does not exceed maxLength.
            return null;
        }
    }
}
