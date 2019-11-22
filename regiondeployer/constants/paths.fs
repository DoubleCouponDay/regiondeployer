module regiondeployer.constants.pathconstants

open regiondeployer.constants.personalpaths

[<Literal>]
let public azureregionsjsonpath = regiondeployersroot + @"azure_regions.json"

[<Literal>]
let public defaultcontractpath = exchangeadaptersroot + @"apicontracts\\independentreserve.json" //all paths must be escaped when used with edgejs!

[<Literal>]
let public pathtocontractsfolder = exchangeadaptersroot + @"built\\apicontracts"

[<Literal>]
let pathtoexchangeadaptersindexfile = exchangeadaptersroot + @"built\\source\\index.js"