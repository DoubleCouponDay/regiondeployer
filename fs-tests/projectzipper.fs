module regiondeployer.tests.projectzipper

open regiondeployer.directoryzipper
open Xunit
open regiondeployer.constants.independentconstants
open regiondeployer.randomnamegenerator
open System.Configuration
open Xunit.Abstractions
open regiondeployer.constants.pathconstants
open regiondeployer.constants.personalpaths
open regiondeployer

type public when_it_zips_up_a_project(inputlogger:ITestOutputHelper) =
    do
        configadapter.initialise();

    member private x.logger = inputlogger

    [<Fact>]
    member public this.it_succeeds() : string =
        let zipfilename = createrandomname(defaultzipname, 20)
        ZipUpProject (regionalfarmersroot, zipfilename, this.logger.WriteLine) 


