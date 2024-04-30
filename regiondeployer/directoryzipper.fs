module regiondeployer.directoryzipper

open System.IO.Compression
open System.IO

[<Literal>]
let public defaultzipname = "regionfarmer"    

///returns the named address of the output zip file
let ZipUpProject (projectpath:string, outputfilename:string, logmessage:string -> unit) : string =
    logmessage ("Zipping project directory... " + projectpath)
    let output = (Directory.GetCurrentDirectory() + "\\" + outputfilename + ".zip") //destination must be different from the source. otherwise, zipfile.createfromdirectory() wont work!

    if File.Exists output then
        File.Delete output

    ZipFile.CreateFromDirectory(projectpath, output)
    logmessage "directory zipped."
    output
