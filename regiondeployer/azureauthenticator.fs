module regiondeployer.azureauthenticator

open System.Configuration
open System.Linq
open Microsoft.Azure.Management.Fluent
open Microsoft.Azure.Management.ResourceManager.Fluent
open System
open regiondeployer.constants.independentconstants
open MoonMachine.Infrastructure.Helpers
open MoonMachine.credentials

let public createauthenticatedgateway() : IAzure = 
    let globalconfig = ProjectCredentials.get.matchingcurrentenvironment()

    let gateway = (SdkContext.AzureCredentialsFactory
        .FromServicePrincipal(
            globalconfig.AzureClientId,
            globalconfig.AzureClientSecret,
            globalconfig.AzureTenantId,
            AzureEnvironment.AzureGlobalCloud
        )
    |> Azure.Authenticate)

    gateway.WithDefaultSubscription()
        
