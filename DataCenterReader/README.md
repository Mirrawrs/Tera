# DataCenterReader
Decrypts, unpacks and deserializes the Data Center.

## Sample usage
```csharp
var dataCenterPath = @"C:\Users\Public\Games\En Masse Entertainment\TERA\Client\S1Game\S1Data\DataCenter_Final_USA.dat";
var gamePath = @"C:\Users\Public\Games\En Masse Entertainment\TERA\Client\Binaries\TERA.exe";
var dataCenter = await DataCenter.Load(dataCenterPath, new GameClientAnalyzer(gamePath));
```

The Data Center is essentially a polytree in which vertices are represented by elements. As such, it can be queried to retrieve data.

```csharp
var root = dataCenter.Root;
var itemNamesById = root.Children
    .Where(element => element.Name == "StrSheet_Item")
    .SelectMany(element => element.Children)
    .ToLookup(child => (int) child["id"], child => (string) child["string"]);
//Note that there are multiple items with the same IDs.
```

## How does it work
After the Data Center file is opened, it's decrypted using Rijndael in CFB mode. The key and IV are hardcoded in the game client and change every build. The block size is 128 bits, but the final block can be shorter, as it doesn't have any padding. Once the data is decrypted, there is a little-endian UInt32 that represents the size of the decompressed payload, followed by the sequence `78 9C` that indicates the header of a zlib-compressed chunk. The latter is decompressed using the deflate method with default compression level.

For more information on how the data is deserialized, review the source code. The deserialization is done through [Lotus](https://github.com/Mirrawrs/Lotus).