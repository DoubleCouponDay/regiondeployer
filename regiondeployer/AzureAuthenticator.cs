using System;
using System.Configuration;
using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using MoonMachine.Infrastructure.Helpers;
using MoonMachine.credentials;

public class AzureAuthenticator
{
    public IAzure CreateAuthenticatedGateway()
    {
        var globalConfig = ProjectCredentials.Get.MatchingCurrentEnvironment();

        var credentials = SdkContext.AzureCredentialsFactory
            .FromServicePrincipal(
                globalConfig.AzureClientId,
                globalConfig.AzureClientSecret,
                globalConfig.AzureTenantId,
                AzureEnvironment.AzureGlobalCloud);

        var authenticatedAzure = Azure.Authenticate(credentials);
        return authenticatedAzure.WithDefaultSubscription();
    }
}