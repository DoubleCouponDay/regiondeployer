using Xunit;
using Xunit.Abstractions;
using regiondeployer.directoryzipper;
using regiondeployer.constants.independentconstants;
using regiondeployer.randomnamegenerator;
using System.Configuration;
using regiondeployer.constants.pathconstants;
using regiondeployer.constants.personalpaths;
using regiondeployer;

namespace Tests
{
    public class WhenItZipsUpAProject
    {
        private readonly ITestOutputHelper _logger;

        public WhenItZipsUpAProject(ITestOutputHelper inputLogger)
        {
            configadapter.initialise();
            _logger = inputLogger;
        }

        [Fact]
        public string ItSucceeds()
        {
            string zipFileName = createrandomname(defaultzipname, 20);
            ZipUpProject(regionalfarmersroot, zipFileName, _logger.WriteLine);
            return zipFileName;
        }
    }
}
