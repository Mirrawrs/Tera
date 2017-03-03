# TeraClient
Contains network-related classes that allow to connect to and communicate with a TERA realm.

## Sample usage
```csharp
var configuration = new TeraClientConfiguration
{
    Username = "username",
    AuthProvider = new EnMasseDataProvider(),
    PacketNamesByOpcode = (await new GameClientAnalyzer("Tera.exe").Analyze()).PacketNamesByOpcode,
    //Specify other client configuration here
};
var dispatcherConfiguration = new DispatcherConfiguration
{
    //Specify module types below
    ModuleTypes = new HashSet<Type>(new[] {typeof(CommandsModule), typeof(ChatModule)}),
    ConverterType = typeof(ObjectConverter)
};
var client = new TeraClient(configuration, dispatcherConfiguration);
try
{
    await client.Run();
}
catch (Exception e)
{
    Console.WriteLine(e);
}
```

An actual usage case is shown in the [SampleBot](https://github.com/Mirrawrs/Tera/tree/master/SampleBot) project.

## Configuration
The `TeraClientConfiguration` class is used to specify parameters related to how the client functions.

Property | Type | Description
--- | --- | --- | ---
**AuthProvider** | `IAuthProvider` | The object that will be used to authenticate the client before logging on.
**BuildVersion** | `int` | The version number that is sent when logging on. The build version must always be up to date.
**PacketNamesByOpcode** | `IReadOnlyDictionary<ushort, string>` | The dictionary that maps opcodes to packet names.
**Password** | `string` | The plaintext user's password that is used to authenticate the client.
**Realm** | `IRealmInfo` | The object that exposes a realm's host, port and name.
**SystemMessageTypes** | `IList<ISystemMessageTypeInfo>` | An ordered list of system message type definitions.
**UnhandledExceptionHandler** | `Action<Exception>` | A delegate invoked when an unhandled exception is thrown from a packet listener.
**Username** | `string` | The user's name that is used to authenticate the client.
See the other projects in this solution or the sample to find out how to obtain the information required from the client to run.

## Dispatcher, modules, listeners and commands
TeraClient uses the [Lotus.Dispatching](https://github.com/Mirrawrs/Lotus/tree/master/Lotus.Dispatching) library to route packets and other objects to listeners contained in types named *modules*. It makes it easy to handle packets that may be received at any moment, as well as more complex data exchanges. Take a look at the module below.

```csharp
public class MyModule
{
    private TeraClient Client { get; [Listener] set; }

    //Example: w SomeUsername "Hello world"
    [Command]
    [ComponentName(Name = "w")]
    public async Task Whisper(string recipient, string message)
    {
        await Client.Send(new C_WHISPER
        {
            RecipientName = recipient,
            Text = message
        });
    }

    [Listener]
    public void OnChatMessage(S_CHAT chatPacket)
    {
        var channel = chatPacket.Channel;
        var sender = chatPacket.Sender;
        var message = chatPacket.Message;
        Console.WriteLine($"[{DateTime.Now:T}] [{channel}] {sender}: {message}");
    }

    [Listener]
    public async Task ChooseCharacterOnLogon(S_LOGIN_ACCOUNT_INFO packet, CancellationToken token)
    {
        await Client.Send(new C_GET_USER_LIST(), token);
        var userListPacket = await Dispatcher.Next<S_GET_USER_LIST>(token);
        var charName = "MyCharacter";
        var chosenUser = userListPacket.Users.SingleOrDefault(user => user.Name == charName);
        if (chosenUser == null) throw new Exception($"There is no character named {charName}.");
        await Client.Send(new C_SELECT_USER {UserId = chosenUser.Id}, token);
    }
}
```

Upon initialization, `TeraClient` creates one module for each type specified in `DispatcherConfiguration.ModuleTypes`. Modules must expose a public parameterless constructor.

### Listeners
Methods marked by the `ListenerAttribute` are invoked as soon as a value of a type assignable from the method's single parameter type is dispatched (or only the exact parameter's type, if `DispatcherConfiguration.ExactTypeOnlyNotifications` is set to `true`). They can optionally accept a `CancellationToken` as second parameter, which is the one passed to `Dispatcher.Notify`. They can return `void` or a type deriving from `Task`, allowing for effortless asynchronous packet exchanges. The following example will call the `OnChatMessage` method.

```csharp
await Dispatcher.Notify(new S_CHAT {Sender = "someUsername", Message = "Hello world"});
```

Properties with setters marked by `ListenerAttribute` behave similarly to methods. As soon as an object with type compatible with the property's is dispatched, the property is set. In `TeraClient.Run`, the client notifies all modules about itself, its dispatcher, the client's configuration and every other module. That makes it easy to inject dependencies into modules as soon as they are created.

### Commands
Methods marked by the `CommandAttribute` are called when the method's name (or, if marked by `ComponentNameAttribute`, the latter's `Name` property) is passed as the `command` argument and the number of parameters (excluding the optional `CancellationToken` at the end) matches the length of the `arguments` array passed to `Dispatcher.Execute`. Like listeners, they can return void or a task. Commands can have any number and type of parameters. If the passed arguments can't be implicitly cast to the parameter types in the command's signature, the `ConverterType` specified in the dispatcher's configuration will be responsible of converting values (see [ObjectConverter](https://github.com/Mirrawrs/Tera/blob/master/SampleBot/ObjectConverter.cs) for a sample). The example below will call the `Whisper` method.

```csharp
await Dispatcher.Execute("w", new object[] {"someUsername", "Hello world"});
```

Note that while a `Notify` call can notify zero, one or more listeners, `Execute` must call exactly one command. Therefore, commands must have different names or different arities (number of parameters).

### The *Next* method
Another useful method that the dispatcher offers is `Next<TValue>()`, which return a task of type `Task<TValue>` that will be completed as soon as an object of type compatible with `TValue` is routed through `Notify`. It essentially creates one-time listeners and it optionally accepts a predicate function that will be used to filter objects and a cancellation token. An example is shown below.

```csharp
//Log on routine
await Client.Send(new C_LOGIN_ARBITER
{
    BuildNumber = 12345,
    Ticket = "Some ticket"
});
//Five seconds max to receive a response.
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var badBuild = Dispatcher.Next<S_INVALID_BUILD_VERSION>(cts.Token);
//The first argument passed to Next is a predicate that allows to filter dispatched objects.
var badTicket = Dispatcher.Next<S_LOGIN_ARBITER>(packet => !packet.Success, cts.Token);
var success = Dispatcher.Next<S_LOGIN_ARBITER>(packet => packet.Success, cts.Token);
var completed = await Task.WhenAny(badTicket, success, badBuild);
//Cancel every other task.
cts.Cancel();
if (completed == badBuild) throw new Exception("Incorrect build version.");
if (completed == badTicket) throw new Exception("Invalid or expired ticket.");
if (completed.IsCanceled) throw new Exception("Incorrect opcodes map or cryptographic parameters.");
//If it gets here, login was successful.
```