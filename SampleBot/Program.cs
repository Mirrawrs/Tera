using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lotus.Dispatching;
using Newtonsoft.Json;
using Tera;
using Tera.Analytics;
using Tera.EnMasse;
using Tera.Net;

namespace SampleBot
{
    //Sample bot that exposes a few functionalities. Explore the source files to learn more.
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Arguments: email password realmName characterName");
                return;
            }
            var (email, password, realmName, characterName) = (args[0], args[1], args[2], args[3]);

            //Configuration for the client created below.
            var configuration = await GetOrCreateConfiguration(email, password, realmName, characterName);
            //The dispatcher is the component responsible of routing packets, system messages, 
            //commands and objects in general to its registered modules. It's configured below.
            var dispatcherConfiguration = new DispatcherConfiguration
            {
                //Register all the listeners and commands in types that derive from MyModuleBase.
                ModuleTypes = new HashSet<Type>(typeof(Program).Assembly.DefinedTypes
                    .Where(type => type.IsSubclassOf(typeof(MyModuleBase)))),
                //Register the type to convert values that will be passed to commands.
                ConverterType = typeof(ObjectConverter)
            };
            var client = new TeraClient(configuration, dispatcherConfiguration);
            //The client will authenticate and connect to the realm and packet dispatching will start.
            var runningTask = client.Run();
            try
            {
                //Wait until the client disconnects or the input task terminates upon unexpected exception.
                await Task.WhenAll(runningTask, ReadInput(client, runningTask));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Press a key to end the program.");
                Console.ReadKey();
            }
        }

        //In this instance, console input is sent to the dispatcher. CommandModule.Execute listens for
        //strings and tries to parse them as commands, then uses the dispatcher to call their respective handlers.
        //Any text input from any source can be used for this process.
        private static async Task ReadInput(TeraClient client, Task runningTask)
        {
            while (true)
            {
                //Console.ReadLine is blocking so wrap it in Task.Run to execute asynchronously.
                var inputTask = Task.Run(() => Console.ReadLine());
                await Task.WhenAny(inputTask, runningTask);
                //Exit the loop if the bot has disconnected.
                if (runningTask.IsCompleted) break;
                var input = inputTask.Result;
                try
                {
                    if (input != null) await client.Dispatcher.Notify(input);
                }
                catch (MyTeraException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception)
                {
                    client.Disconnect();
                    throw;
                }
            }
        }

        //Instead of pulling live game data every time the program runs, the version-specific information is
        //cached to significantly speed up loading times.
        private static async Task<MyTeraClientConfig> GetOrCreateConfiguration(
            string email,
            string password,
            string realmName,
            string characterName)
        {
            //Path constants
            const string configurationPath = "config.json";
            const string teraPath = @"C:\Users\Public\Games\En Masse Entertainment\TERA\Client";
            const string gamePath = teraPath + @"\Binaries\TERA.exe";
            const string dataCenterPath = teraPath + @"\S1Game\S1Data\DataCenter_Final_USA.dat";

            //Since the configuration class contains some interface types, type names must be serialized along with their data.
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};

            //Initialize the component that will authenticate the user on EnMasse servers and provide a list of realms.
            var emeProvider = new EnMasseDataProvider();

            if (!File.Exists(configurationPath))
            {
                //Scan the game for packet opcodes, data center keys, etc.
                var gameInfo = await new GameClientAnalyzer(gamePath).Analyze();
                //Unpack the data center and load it in memory.
                var dataCenter = await DataCenter.Load(dataCenterPath, gameInfo.DataCenterKey, gameInfo.DataCenterIv);

                //Mapping system message types is useful because in many instances they're the only kind of acknowledgment
                //that the server sends to certain requests.
                //Below is a set of a few parent element names that contain system message type definitions. Note that there may be more.
                var systemMessageTypesContainers = new HashSet<string>(new[]
                {
                    "StrSheet_SystemMessage", "StrSheet_Party", "StrSheet_PetitionTitle", "StrSheet_Option",
                    "StrSheet_Masstige", "StrSheet_InGameStore", "StrSheet_Guild", "StrSheet_GameStat",
                    "StrSheet_DungeonMatching", "StrSheet_AdminTool"
                });
                var systemMessageTypesByName = dataCenter.Root.Children
                    .Where(element => systemMessageTypesContainers.Contains(element.Name))
                    .SelectMany(element => element.Children)
                    //Some system message types are defined more than once, use a lookup to avoid duplicate key issues.
                    .ToLookup(element => element["readableId"] as string, element => element["string"] as string);
                var systemMessageTypes = gameInfo.SystemMessageReadableIds
                    .Select(name => new SystemMessageTypeInfo(name, systemMessageTypesByName[name].FirstOrDefault()))
                    .Cast<ISystemMessageTypeInfo>()
                    .ToList();

                //In order to login on a realm, an up to date build number must be sent.
                var buildVersion = (int) dataCenter.Root.Children.First(e => e.Name == "BuildVersion")["version"];

                //Packets are identified by a 16-bit integer that changes every patch. 
                //The client needs this information to communicate with the realm.
                var packetNamesByOpcode = gameInfo.PacketNamesByOpcode;

                //Create the configuration object and store it. When a new update lands, deleting the file is enough 
                //for the bot to know that it must reanalyze the client and the data center.
                var newConfiguration = new MyTeraClientConfig
                {
                    AuthProvider = emeProvider,
                    BuildVersion = buildVersion,
                    PacketNamesByOpcode = packetNamesByOpcode,
                    SystemMessageTypes = systemMessageTypes
                };
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(newConfiguration, settings));
            }
            var configurationJson = File.ReadAllText(configurationPath);
            var configuration = JsonConvert.DeserializeObject<MyTeraClientConfig>(configurationJson, settings);

            //Since this data is subject to change or sensitive, set it at runtime rather than saving it.
            var realms = await emeProvider.GetRealms();
            configuration.Realm = realms.Single(realm => realm.Name == realmName);
            configuration.Username = email;
            configuration.Password = password;
            configuration.CharacterName = characterName;
            configuration.UnhandledExceptionHandler = e => Console.WriteLine(e.Message);
            return configuration;
        }
    }
}