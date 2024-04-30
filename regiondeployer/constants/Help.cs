namespace RegionDeployer
{
    public class Helper
    {
        public const string HelpString = 
            "regional deployer requires 4 arguments:\n" +
            "    -regionalfarmersrootpath:   Absolute path to the regionalfarmer project's release directory\n" +
            "    -ContractsFolderPath:       Absolute path to the exchangeadapter project's apicontracts\n" +
            "    -NumberOfFarmers:           Number of farmers to deploy\n";

        public const string NoArgumentsString = "4 arguments expected, use -h for help";
    }
}