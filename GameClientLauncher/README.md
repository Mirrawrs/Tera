# GameClientLauncher
Allows to start the game client without relying on the launcher released by the publisher. That also means that it allows using a custom server list or launch information without having to modify the environment or the client.

## Sample usage
```csharp
var launcherConfig = new LauncherConfiguration
{
    Username = "email",
    Password = "password",
    LaunchInfoProvider = new EnMasseDataProvider(),
    TeraLauncherPath = @"C:\Users\Public\Games\En Masse Entertainment\TERA\Client\TL.exe"
};
var launcher = new Launcher(launcherConfig);
```

## How does it work
It launches the TL.exe process and it communicates with it through the `WM_COPYDATA` window message. TL.exe queries information such as server list and launch info and, once obtained, starts the actual game client and dispatches the data. Note that patching the game is not an included (or planned) feature.
