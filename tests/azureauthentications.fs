module regiondeployer.tests.azureauthentication

open regiondeployer.azureauthenticator
open Microsoft.Azure.Management.Fluent
open System.Diagnostics
open Xunit
open Xunit.Abstractions
open System.Configuration
open MoonMachine.credentials
open regiondeployer

type public when_it_authenticates_to_azure(inputlogger:ITestOutputHelper) =
    do
        configadapter.initialise();

    member private x.logger = inputlogger

    [<Fact>]
    member public this.it_succeeds() : IAzure =
        createauthenticatedgateway()

    [<Fact>]
    member public this.gateways_principal_matches_my_input_values() =
        let globalconfig = ProjectCredentials.get.matchingcurrentenvironment()

        Assert.True(
            this.it_succeeds()
                .SubscriptionId = globalconfig.AzureSubscriptionId
        )
            
            

