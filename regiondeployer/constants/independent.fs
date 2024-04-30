module regiondeployer

open System.Configuration

[<Literal>]
let public valueseparator = ": " 

[<Literal>]
let public ipaccesskeyname = "ipstackkey"

[<Literal>]
let public accesskeyparamkey = "?access_key="

[<Literal>]
let public placeholderipstackaddress = @"http://api.ipstack.com/google.com" + accesskeyparamkey + "26928c3a15335fb85a8418e7dff2c3bf"

[<Literal>]
let jsonmimetype = "application/json"
