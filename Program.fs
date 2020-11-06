// Learn more about F# at http://fsharp.org

open System
open FSharp.Data // http.Request

type Credentials = {
    UserName: string
    Key: string
    Ip: string
  }

type ApiResponseDomainCheck = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?>
<ApiResponse Status="OK" xmlns="http://api.namecheap.com/xml.response">
    <Errors />
    <Warnings />
    <RequestedCommand>namecheap.domains.check</RequestedCommand>
    <CommandResponse Type="namecheap.domains.check">
        <DomainCheckResult Domain="arwn.com" Available="true" ErrorNo="0" Description="" IsPremiumName="false" PremiumRegistrationPrice="0" PremiumRenewalPrice="0" PremiumRestorePrice="0" PremiumTransferPrice="0" IcannFee="0" EapFee="0.0" />
        <DomainCheckResult Domain="arwn.co" Available="true" ErrorNo="0" Description="" IsPremiumName="false" PremiumRegistrationPrice="0" PremiumRenewalPrice="0" PremiumRestorePrice="0" PremiumTransferPrice="0" IcannFee="0" EapFee="0.0" />
    </CommandResponse>
    <Server>PHX01SBAPIEXT02</Server>
    <GMTTimeDifference>--5:00</GMTTimeDifference>
    <ExecutionTime>0.522</ExecutionTime>
</ApiResponse>""">

let checkDomains domains =
    domains
    |> String.concat ","
    |> sprintf "&Command=namecheap.domains.check&DomainList=%s"


let makeURI c rest =
    sprintf
        "https://api.sandbox.namecheap.com/xml.response?ApiUser=%s&ApiKey=%s&UserName=%s&ClientIp=%s%s"
        c.UserName
        c.Key
        c.UserName
        c.Ip
        rest

let domainNamesAreAvailable domainName c =
    let tryParse x =
        try Ok (ApiResponseDomainCheck.Parse x)
        with ex -> Error "Could not parse the xml recieved from the namecheap api"
    domainName
    |> checkDomains
    |> makeURI c
    |> Http.RequestString
    |> tryParse

let getCredentials () =
    let userName = Environment.GetEnvironmentVariable "ncqUserName"
    let key = Environment.GetEnvironmentVariable "ncqKey"
    let ip = Environment.GetEnvironmentVariable "ncqIp"
    if isNull userName || isNull key || isNull ip then
        Error "Could not read credentials, please set the environment variables [ncqUserName, ncqKey, ncqIP]"
    else
        Ok { UserName = userName
             Key = key
             Ip = ip
           }

// TODO: ew type signatures grodey dude
let prettyPrintDomains (parsedDomains:ApiResponseDomainCheck.ApiResponse) =
    let prettyPrint (r:ApiResponseDomainCheck.DomainCheckResult) =
        let available = (if r.Available then "" else "not ") + "available"
        let cost = if r.Available && r.IsPremiumName then "at $" + r.PremiumRegistrationPrice.ToString() else ""
        printfn "%s is %s %s" r.Domain available cost
    Seq.iter prettyPrint parsedDomains.CommandResponse.DomainCheckResults

[<EntryPoint>]
let main argv =
    getCredentials()
    |> Result.bind (domainNamesAreAvailable argv)
    |> function
        | Ok parsedDomains -> prettyPrintDomains parsedDomains |> ignore; 0
        | Error e -> printfn "%s" e; 1
