module regiondeployer.constants.helpconstants

[<Literal>]
let public helpstring = @"
    regional deployer requires 4 arguments:
        -regionalfarmersrootpath:   Absolute path to the regionalfarmer project's release directory
        -ContractsFolderPath:       Absolute path to the exchangeadapter project's apicontracts
        -NumberOfFarmers:           Number of farmers to deploy
"

[<Literal>]
let public noargumentsstring = "4 arguments expected, use -h for help"
