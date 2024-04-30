// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open regiondeployer
open regiondeployer.azureauthenticator
open regiondeployer.cloudactions.resourcegroup
open regiondeployer.databaseseeder
open regiondeployer.databasecleaner
open regiondeployer.logger
open MoonMachine.credentials
open System
open MoonMachine.Infrastructure.models

[<EntryPoint>]
let main argv = 
    initialize()
    configadapter.initialise()
    let currentconfig = ProjectCredentials.get.matchingcurrentenvironment()
    let currentenvironment = Enum.GetName(currentconfig.GetType(), currentconfig.currentenvironment)
    logtoconsoleandfile("cleaning deployment and initial state of environment: " + currentenvironment)
    deletefarmergroupsmatchingcurrentenvironmentmode logtoconsoleandfile
    databasecleaner.removeinitialstate(logtoconsoleandfile)
    0 // return an integer exit code
