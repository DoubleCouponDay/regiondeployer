namespace regiondeployer;

public class Paths
{
    // Assuming 'regionDeployersRoot' and 'exchangeAdaptersRoot' are defined elsewhere in your C# project
    // and are accessible from this class. Their definitions are not provided in the F# code snippet.
    private static readonly string regionDeployersRoot = "YourRegionDeployersRootPath";
    private static readonly string exchangeAdaptersRoot = "YourExchangeAdaptersRootPath";

    public const string AzureRegionsJsonPath = regionDeployersRoot + @"azure_regions.json";
    public const string DefaultContractPath = exchangeAdaptersRoot + @"apicontracts\independentreserve.json";
    public const string PathToContractsFolder = exchangeAdaptersRoot + @"built\apicontracts";
    public const string PathToExchangeAdaptersIndexFile = exchangeAdaptersRoot + @"built\source\index.js";
}