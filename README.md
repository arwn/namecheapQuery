# namecheapQuery

Check the availability of domain names using the namecheap api.

You will need a namecheap sandbox api access key and username. You also will need to whitelist your ip on namecheap's site: https://ap.www.sandbox.namecheap.com/settings/tools/apiaccess/

example:
```
% ncqUserName="arwn"
% ncqKey="aaaaaaaaaaaaaaaaaaaaaaaaaa"
% ncqIP="666.777.888.999"
% dotnet run "arwn.com"
arwn.com avaliable: True
```