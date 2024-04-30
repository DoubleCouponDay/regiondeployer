module regiondeployer

open Microsoft.Azure.Management.Network.Fluent.Models
open Microsoft.Azure.Management.AppService.Fluent.Models
open System
open Microsoft.Rest.Azure
open System.Threading.Tasks
open serversidearchitecture.common
open MoonMachine.Core

let public printclouderror(inputerror:Exception, logmessage:string -> unit) : unit =
    let errorutils : errorutilities = new errorutilities()
    let log = errorutils.createlog(inputerror, String.Empty, false)
    String.concat valueseparator [log.Message; log.Stacktrace]
    |> logmessage    

let public raiseclouderror(inputerror:Exception) : unit =
    let errorutils : errorutilities = new errorutilities()
    let log = errorutils.createlog(inputerror, String.Empty, false)
    String.concat valueseparator [log.Message; log.Stacktrace]
    |> failwith