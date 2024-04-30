using MoonMachine.credentials;
using Microsoft.FSharp.Linq;
using System;
using MoonMachine.Infrastructure.models;
using MoonMachine.Core.interop.models;

public class ConfigAdapter
{
    public void Initialise()
    {
        #if DEBUG
            ProjectCredentials.Get.GlobalOverride(new Nullable<mmenvironment>(mmenvironment.debug));
        #else
            ProjectCredentials.Get.GlobalOverride(new Nullable<mmenvironment>(mmenvironment.prod));
        #endif
    }
}