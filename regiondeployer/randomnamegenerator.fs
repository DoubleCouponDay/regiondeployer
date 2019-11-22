module regiondeployer.randomnamegenerator

open RandomStringGenerator

let public createrandomname(prependage:string, totallength:int) : string =
    let randomamount = totallength - prependage.Length
    let generator = new StringGenerator()
    (prependage + generator.GenerateString(randomamount))

