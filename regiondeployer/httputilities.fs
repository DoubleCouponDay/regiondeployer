module HttpUtilities

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Text
open System.Threading.Tasks



let basicauthseparator = ":"
let basicauthkey = "Authorization"
let dummyurl = Uri("http://localhost")

let getjsoncontent (json: string) =
    new StringContent(json, Encoding.UTF8, "application/json") // Assuming coreconstants.jsonmimetype is "application/json"

let createrequestmessage (url: string) (method: HttpMethod) (content: HttpContent) (headers: KeyValuePair<string, string>[]) =
    let request = new HttpRequestMessage(method, Uri(url))
    request.Content <- content
    headers |> Array.iter (fun kvp -> request.Headers.Add(kvp.Key, kvp.Value))
    request

let Clone (req: HttpRequestMessage) =
    let clone = new HttpRequestMessage(req.Method, req.RequestUri)
    clone.Content <- req.Content
    clone.Version <- req.Version
    let sequence = req.Options.AsEnumerable() 
    
    Seq.iter(fun (current : KeyValuePair<string, obj>) -> 
        clone.Options.Append(new KeyValuePair<string, obj>(current.Key, current.Value)) |> ignore)
        sequence
    
    let oldheaders: List<KeyValuePair<string, IEnumerable<string>>> = req.Headers.ToList()
    
    for current in oldheaders do
        clone.Headers.TryAddWithoutValidation(current.Key, current.Value) |> ignore

    clone


let createbasicauthheader (username: string) (password: string) =
    let input = username + basicauthseparator + password
    let bytes = Encoding.UTF8.GetBytes(input)
    let output = Convert.ToBase64String(bytes)
    KeyValuePair<string, string>(basicauthkey, "Basic " + output)
