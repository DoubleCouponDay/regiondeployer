namespace RegionDeployer
{
    using Microsoft.Azure.Management.AppService.Fluent;
    using Microsoft.Azure.Management.AppService.Fluent.AppServicePlan.Definition;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServicePlan
    {
        private const string BuggedServicePlanName = "buggedappservice";
        private const string DefaultLinuxRegion = "westus";
        private readonly IAzure azureGateway;

        public ServicePlan(IAzure azure)
        {
            azureGateway = azure;
        }

        public IAppServicePlan CreateLinuxServicePlan(string resourceGroupName, Action<string> logMessage)
        {
            var log = $"creating app service plan of name: {BuggedServicePlanName}";
            logMessage(log);

            var output = azureGateway.AppServices.AppServicePlans
                .Define(BuggedServicePlanName)
                .WithRegion(DefaultLinuxRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithPricingTier(PricingTier.BasicB1)
                .WithOperatingSystem(OperatingSystem.Linux)
                .Create();

            logMessage("app service plan created.");
            return output;
        }

        public void DeleteServicePlan(string planId, Action<string> logMessage)
        {
            var log = $"deleting app service plan of Id: {planId}";
            logMessage(log);

            azureGateway.AppServices.AppServicePlans.DeleteById(planId);

            logMessage("app service plan deleted.");
        }
    }
}