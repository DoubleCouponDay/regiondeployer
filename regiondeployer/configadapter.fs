module regiondeployer

open MoonMachine.credentials
open Microsoft.FSharp.Linq
open System
open MoonMachine.Infrastructure.models
open MoonMachine.Core.interop.models

let public initialise() : unit =
    #if DEBUG
        ProjectCredentials.get.globaloverride(new Nullable<mmenvironment>(mmenvironment.debug));
    #else
        ProjectCredentials.get.globaloverride(new Nullable<mmenvironment>(mmenvironment.prod));
    #endif
