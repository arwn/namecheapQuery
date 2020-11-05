// Learn more about F# at http://fsharp.org

open System
open FSharp.Data // http.Request

type Credentials = {
    UserName: string
    Key: string
    Ip: string
  }

type ApiResponseDomainCheckResult = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?>
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

let commandDomainsCheck domains =
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
    domainName
    |> commandDomainsCheck
    |> makeURI c
    |> Http.RequestString
    |> try ApiResponseDomainCheckResult.Parse with e -> failwith "couldnt parse result from namecheap's api"
    |> fun x -> x.CommandResponse.DomainCheckResults
    |> Seq.map (fun x -> x.Available.ToString())

let getCredentials () =
    let userName = Environment.GetEnvironmentVariable "ncqUserName"
    let key = Environment.GetEnvironmentVariable "ncqKey"
    let ip = Environment.GetEnvironmentVariable "ncqIp"
    if isNull userName || isNull key || isNull ip then
        failwith "Could not read credentials, please set the environment variables [ncqUserName, ncqKey, ncqIP]"
    else
        { UserName = userName
          Key = key
          Ip = ip
        }

[<EntryPoint>]
let main argv =
    getCredentials()
    |> domainNamesAreAvailable argv
    |> Seq.zip argv
    |> Seq.iter (fun (f,s) -> printfn "%s avaliable: %s" f s)
    0