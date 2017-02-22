# GameClientAnalyzer
Retrieves packet type names by opcode, an ordered list of system message names and the key and IV to decrypt the Data Center.

## Sample usage
```csharp
var gamePath = @"C:\Users\Public\Games\En Masse Entertainment\TERA\Client\Binaries\TERA.exe";
var analyzer = new GameClientAnalyzer();
var result = await analyzer.Analyze(gamePath);
```

## How does it work
It creates the TERA.exe process in a suspended state and injects itself into it. Hooks one of the first routines used by the unpacked client and, once deobfuscation is complete, scans the memory of the main module for the patterns of relevant data; the results are reported back to the injector through inter-process-communication and the TERA process is terminated.
