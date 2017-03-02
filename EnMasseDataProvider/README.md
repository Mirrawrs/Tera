# EnMasseDataProvider
A basic interface to use EnMasse's online services.

## Sample usage
```csharp
var emeProvider = new EnMasseDataProvider();
var realms = await emeProvider.GetRealms();
var authTicket = await emeProvider.Authenticate("email", "password");
```

It implements the `IAuthProvider` interface, used by [TeraClient](https://github.com/Mirrawrs/Tera/tree/master/TeraClient) to log on a realm.

## How does it work
It mimics the calls to APIs invoked by the TERA launcher. Note that it can't authenticate accounts with [Account Armor](https://account.enmasse.com/users/account/profile) turned on.