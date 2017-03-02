using System;
using System.Threading;
using System.Threading.Tasks;
using Lotus.Dispatching;
using Lotus.Dispatching.Attributes;
using Tera.Packets;

namespace Tera.Net
{
    /// <summary>
    ///     Provides common functionalities for a <see cref="TeraClient" />. Instantiated through a
    ///     <see cref="Lotus.Dispatching.Dispatcher" />.
    /// </summary>
    internal sealed class CoreModule
    {
        private SystemMessageReader systemMessageReader;
        private TeraClient Client { get; [Listener] set; }
        private Dispatcher Dispatcher { get; [Listener] set; }

        [Listener]
        private void OnConfigurationReceived(TeraClientConfiguration configuration)
            => systemMessageReader = new SystemMessageReader(configuration.SystemMessageTypes);

        [Listener]
        private Task KeepAlive(S_PING ping) => Client.Send(new C_PONG());

        [Listener]
        private Task ReadSystemMessage(S_SYSTEM_MESSAGE systemMessage, CancellationToken token)
            => Dispatcher.Notify(systemMessageReader.Read(systemMessage.Content), token);

        public async Task Login(string ticket, int buildVersion)
        {
            await Client.Send(new C_LOGIN_ARBITER
            {
                BuildNumber = buildVersion,
                Ticket = ticket
            });
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var badBuild = Dispatcher.Next<S_INVALID_BUILD_VERSION>(cts.Token);
            var badTicket = Dispatcher.Next<S_LOGIN_ARBITER>(packet => !packet.Success, cts.Token);
            var success = Dispatcher.Next<S_LOGIN_ARBITER>(packet => packet.Success, cts.Token);
            var completed = await Task.WhenAny(badTicket, success, badBuild);
            cts.Cancel();
            if (completed == badBuild) throw new Exception("Incorrect build version.");
            if (completed == badTicket) throw new Exception("Invalid or expired ticket.");
            if (completed.IsCanceled) throw new Exception("Incorrect opcodes map or cryptographic parameters.");
        }
    }
}