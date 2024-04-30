module regiondeployer

open System
open System.Text

let public createrandomname(prependage:string, length: int) : string =
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    let totallength = (prependage.Length + length)
    let selected: char array = Array.zeroCreate totallength
    let random = new Random()

    for index in seq { 0 .. 1 .. length } do
        let randomNum = random.Next(chars.Length)
        let nextChar = chars[randomNum]
        Array.set selected index nextChar

    selected.ToString()
