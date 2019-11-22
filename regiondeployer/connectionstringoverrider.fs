module regiondeployer.connectionstringoverrider

open MoonMachine.Infrastructure.Identity

let public overrideconnectionstring(connectionstring:string option): ApplicationDbContext = 
    if connectionstring.IsSome then
        new ApplicationDbContext(connectionstring.Value)

    else
        new ApplicationDbContext()