namespace RegionDeployer;

using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using regiondeployer;

public class AzureAuthenticator
{
    public IAzure CreateAuthenticatedGateway(ConfigFile config)
    {
        var credentials = SdkContext.AzureCredentialsFactory
            .FromServicePrincipal(
                config.AzureClientId,
                config.AzureClientSecret,
                config.AzureTenantId,
                AzureEnvironment.AzureGlobalCloud);

        var authenticatedAzure = Azure.Authenticate(credentials);
        return authenticatedAzure.WithDefaultSubscription();
    }
}
