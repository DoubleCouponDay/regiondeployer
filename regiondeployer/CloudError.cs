using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using System;
using Microsoft.Rest.Azure;
using System.Threading.Tasks;
using serversidearchitecture.common;
using MoonMachine.Core;

public class CloudErrorHelper
{
    public void PrintCloudError(Exception inputError, Action<string> logMessage)
    {
        var errorUtils = new ErrorUtilities();
        var log = errorUtils.CreateLog(inputError, string.Empty, false);
        var logOutput = string.Concat(log.Message, log.Stacktrace);
        logMessage(logOutput);
    }

    public void RaiseCloudError(Exception inputError)
    {
        var errorUtils = new ErrorUtilities();
        var log = errorUtils.CreateLog(inputError, string.Empty, false);
        var logOutput = string.Concat(log.Message, log.Stacktrace);
        throw new Exception(logOutput);
    }
}