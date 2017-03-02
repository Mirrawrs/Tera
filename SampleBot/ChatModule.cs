using System;
using System.Threading.Tasks;
using Lotus.Dispatching.Attributes;
using Tera.Packets;

namespace SampleBot
{
    //Module responsible of handling chat commands and listeners.
    internal class ChatModule : MyModuleBase
    {
        //The method defined below can be called as a regular method but also invoked as a command with prefix "w".
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

        //Uncomment the following block to write timestamped messages to the console.
        /*
        [Listener]
        public void OnChatMessage(S_CHAT chatPacket)
        {
            var channel = chatPacket.Channel;
            var sender = chatPacket.Sender;
            var message = chatPacket.Message;
            Console.WriteLine($"[{DateTime.Now:T}] [{channel}] {sender}: {message}");
        }
        */
    }
}