# Sample bot
A basic client implementation with a few modules that can be used as a base for more complex programs.

## Sample usage
```
SampleBot.exe email password "Server name" CharacterName
```

Once the "Entering world" message appears, use the `whois characterName` sample command to find information about the specified character.

## How does it work
First, if a cached configuration file is not found, it scans the game client and data center for version-specific data, which is then saved for faster loading times. Then it creates the configuration for the dispatcher used by the client, which is instantiated afterwards along with the modules specified in the setup. Once `TeraClient.Run()` is called, it authenticates against the EnMasse servers and it logs on the specified realm. At this point inbound packets and system messages are being dispatched to the listening module components; `RealmEnterModule`, for instance, listens for `S_LOGIN_ACCOUNT_INFO` to instruct the server to enter the world with the character whose name was specified in the client's configuration. Simultaneously, standard input is routed through the dispatcher to the `CommandModule` instance, which parses it and turns it into a command which is, again, sent to command handlers registered with the dispatcher.

The [dispatcher](https://github.com/Mirrawrs/Lotus/tree/master/Lotus.Dispatching) is a component that uses Reflection and expression trees to dynamically create and invoke multicast events which wrap certain attribute-marked methods contained inside of modules, which are in turn specified in the dispatcher's configuration. Essentially, it's a simple, efficient IoC container.