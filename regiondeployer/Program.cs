using System;
using System.IO;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Rest.Azure;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Threading;

namespace regiondeployer
{
    class Program
    {
        private static void CrashIfNotAbsolute(string inputPath)
        {
            LogToConsoleAndFile($"checking if {inputPath} is absolute");

            if (!Path.IsPathRooted(inputPath))
            {
                LogToConsoleAndFile($"path {inputPath} is not absolute");
                throw new Exception($"path {inputPath} is not absolute");
            }
            else
            {
                LogToConsoleAndFile("path is absolute");
            }
        }

        private static Arguments TestInputs(string[] inputs)
        {
            LogToConsoleAndFile($"validating inputs... {string.Join(", ", inputs)}");

            switch (inputs.Length)
            {
                case 0:
                    LogToConsoleAndFile(noArgumentsString);
                    Environment.Exit(0);
                    break;
                case 1 when inputs[0] == "-h":
                    LogToConsoleAndFile(helpString);
                    Environment.Exit(0);
                    break;
                case < 3:
                    LogToConsoleAndFile(noArgumentsString);
                    Environment.Exit(0);
                    break;
                default:
                    LogToConsoleAndFile("Arguments found");
                    break;
            }

            var args = new Arguments
            {
                RegionalFarmersRootPath = inputs[0],
                ContractsFolderPath = inputs[1],
                NumberOfFarmers = int.Parse(inputs[2])
            };

            foreach (var path in new[] { inputs[0], inputs[1] })
            {
                CrashIfNotAbsolute(path);
            }

            LogToConsoleAndFile("inputs validated.");
            return args;
        }

        static int Main(string[] argv)
        {
            Initialize();
            LogToConsoleAndFile($"{DateTime.UtcNow:u} starting deployment procedure...");

            try
            {
                var arguments = TestInputs(argv);
                var zipFilePath = ZipUpProject(arguments.RegionalFarmersRootPath, defaultZipName, LogToConsoleAndFile);
                ConfigAdapter.Initialize();

                var config = ProjectCredentials.GetMatchingCurrentEnvironment();

                LogToConsoleAndFile($"deploying using a {Enum.GetName(typeof(EnvironmentType), config.CurrentEnvironment)} environment");
                LogToConsoleAndFile($"using connection string {config.ConnectionString}");

                ToggleMasterFarmer(false, LogToConsoleAndFile);

                DatabaseCleaner.RemoveInitialState(LogToConsoleAndFile);
                DeleteFarmerGroupsMatchingCurrentEnvironmentMode(LogToConsoleAndFile);

                DatabaseSeeder.AddInitialState(arguments.ContractsFolderPath, LogToConsoleAndFile);

                var group = CreateRandomResourceGroup(defaultRegion, LogToConsoleAndFile);
                var storageAccount = CreateRandomNamedStorageAccount(group, LogToConsoleAndFile);

                FixLinuxAppBug(group, LogToConsoleAndFile, storageAccount);

                var deploymentOutcome = DeployArray(arguments, zipFilePath, true, group, LogToConsoleAndFile, storageAccount);

                if (deploymentOutcome)
                    ToggleMasterFarmer(true, LogToConsoleAndFile);
                else
                    LogToConsoleAndFile("deploying failed.");
            }
            catch (Exception error)
            {
                PrintCloudError(error, LogToConsoleAndFile);
                throw;
            }

            LogToConsoleAndFile("procedure finished.");
            return 0;
        }
    }
}