using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lotus.Dispatching;
using Tera.Net.Cryptography;
using Tera.Packets;

namespace Tera.Net
{
    /// <summary>
    ///     Provides client connections for TERA realm services.
    /// </summary>
    public class TeraClient : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly TeraClientConfiguration configuration;
        private readonly Action<Exception> exceptionHandler;
        private readonly byte[] inputBuffer = new byte[ushort.MaxValue];
        private readonly byte[] outputBuffer = new byte[ushort.MaxValue];
        private readonly SemaphoreSlim sendingSemaphore = new SemaphoreSlim(1, 1);
        private readonly TeraSerializer serializer;
        private readonly TcpClient tcpClient = new TcpClient();
        private TeraCryptoStream stream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeraClient" /> class using the specified configuration.
        /// </summary>
        /// <param name="clientConfiguration">The configuration for the <see cref="TeraClient" />.</param>
        /// <param name="dispatcherConfiguration">
        ///     The configuration for the <see cref="Lotus.Dispatching.Dispatcher" /> that will
        ///     be used internally to dispatch messages.
        /// </param>
        public TeraClient(
            TeraClientConfiguration clientConfiguration,
            DispatcherConfiguration dispatcherConfiguration)
        {
            configuration = clientConfiguration;
            serializer = new TeraSerializer(new MemoryStream(outputBuffer), configuration);
            exceptionHandler = configuration.UnhandledExceptionHandler ??
                               throw new ArgumentNullException(nameof(configuration.UnhandledExceptionHandler));
            dispatcherConfiguration.ModuleTypes.Add(typeof(CoreModule));
            Dispatcher = new Dispatcher(dispatcherConfiguration);
        }

        /// <summary>
        ///     Gets the dispatcher associated with this client.
        /// </summary>
        public Dispatcher Dispatcher { get; }

        /// <summary>Disconnects from the realm and releases the resources acquired by the client.</summary>
        public void Dispose()
        {
            Disconnect();
            sendingSemaphore.Dispose();
            tcpClient.Dispose();
            stream.Dispose();
            cancellationTokenSource.Dispose();
            Dispatcher.Dispose();
        }

        /// <summary>
        ///     Terminates the connection to the TERA realm.
        /// </summary>
        public void Disconnect()
        {
            cancellationTokenSource.Cancel();
            tcpClient.Client.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        ///     Connects to a realm using the credentials specified in the configuration.
        /// </summary>
        /// <returns>An asynchronous task that completes once the client disconnects.</returns>
        public async Task Run()
        {
            var initializeModules = new object[] {Dispatcher, this, configuration}
                .Concat(Dispatcher.Modules)
                .Select(o => Dispatcher.Notify(o));
            await Task.WhenAll(initializeModules);
            var ticket = await configuration.AuthProvider.Authenticate(configuration.Username, configuration.Password);
            await tcpClient.ConnectAsync(configuration.Realm.Host, configuration.Realm.Port);
            stream = await TeraCryptoStream.Initialize(tcpClient.GetStream());
            var readPackets = ReadPackets();
            try
            {
                //Any exception thrown by the login method will cause the client to disconnect.
                await Dispatcher.Get<CoreModule>().Login(ticket, configuration.BuildVersion);
                await readPackets;
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        ///     Sends a packet to the server.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns></returns>
        public async Task Send(Packet packet, CancellationToken cancellationToken = default(CancellationToken))
        {
            await sendingSemaphore.WaitAsync(cancellationToken);
            var packetSize = serializer.WritePacket(packet);
            await stream.WriteAsync(outputBuffer, 0, packetSize, cancellationToken);
            sendingSemaphore.Release();
        }

        private async Task ReadPackets()
        {
            var cancellationToken = cancellationTokenSource.Token;
            try
            {
                var deserializer = new TeraSerializer(new MemoryStream(inputBuffer), configuration);
                while (!cancellationToken.IsCancellationRequested)
                {
                    const int headerLength = sizeof(ushort);
                    await stream.ReadExactly(inputBuffer, 0, headerLength, cancellationToken);
                    var packetLength = BitConverter.ToUInt16(inputBuffer, 0);
                    await stream.ReadExactly(inputBuffer, headerLength, packetLength - headerLength, cancellationToken);
                    var packet = deserializer.ReadPacket();
                    //The following call isn't awaited because listeners that wait on future packets would cause a deadlock.
                    NotifyAsync(packet, cancellationToken);
                }
            }
            //If cancellation is requested, end of stream represents a graceful disconnection.
            catch (EndOfStreamException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        private async void NotifyAsync(object value, CancellationToken cancellationToken)
        {
            try
            {
                await Dispatcher.Notify(value, cancellationToken);
            }
            catch (Exception e)
            {
                exceptionHandler(e);
            }
        }
    }
}