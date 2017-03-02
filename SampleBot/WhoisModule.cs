using System;
using System.Threading;
using System.Threading.Tasks;
using Lotus.Dispatching.Attributes;
using Tera.Packets;
using Tera.SystemMessages;

namespace SampleBot
{
    //Module that allows the user to get basic information about a character.
    internal class WhoisModule : MyModuleBase
    {
        //When TeraClient.Run is called, the dispatcher notifies listeners about:
        // - The TeraClient instance
        // - The Dispatcher instance
        // - The client configuration instance
        // - Every module instance which type was specified in DispatcherConfiguration.ModuleTypes
        //It's an easy way to inject dependencies between modules.
        private ChatModule Chat { get; [Listener] set; }

        //Prints information about the specified character.
        [Command]
        public async Task Whois(string characterName)
        {
            await Client.Send(new C_ASK_INTERACTIVE {Username = characterName});
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var token = cancellationTokenSource.Token;
            try
            {
                var info = await Dispatcher.Next<S_ANSWER_INTERACTIVE>(token);
                var isOnline = await IsOnline(characterName, token);
                var message = $"{info.Username}: lv.{info.Level} {info.ModelInfo} ({(isOnline ? "On" : "Off")}line)";
                Console.WriteLine(message);
            }
            catch (OperationCanceledException)
            {
                throw new MyTeraException($"Character {characterName} not found.");
            }
        }

        //An example of a more complicated asynchronous task to check if an existing character is online without them receiving a whisper.
        private async Task<bool> IsOnline(string characterName, CancellationToken token)
        {
            //The client tries to block the character.
            await Client.Send(new C_BLOCK_USER {UserName = characterName}, token);
            //The server can respond with a confirmation packet...
            var blocked = Dispatcher.Next<S_ADD_BLOCKED_USER>(token);
            //... Or with a failure system message.
            var failure = Dispatcher.Next<SMT_GENERAL_INVALID_TARGET>(token);
            //Await either.
            var complete = await Task.WhenAny(blocked, failure);
            //If the blocking procedure failed, throw an exception.
            if (complete == failure) throw new MyTeraException($"Failed to block character {characterName}.");
            //Otherwise, whisper the character that was just blocked.
            await Chat.Whisper(characterName, "Hi");
            //The server can respond with a system message informing that a blocked character can't be whispered...
            var userBlocked = Dispatcher.Next<SMT_CANT_CONTRACT_BLOCK_USER>(token);
            //... Or a message informing that the character is not online.
            var userOffline = Dispatcher.Next<SMT_GENERAL_NOT_IN_THE_WORLD>(token);
            //Await either.
            complete = await Task.WhenAny(userOffline, userBlocked);
            //The user is online if the former task was the one that completed.
            var isOnline = complete == userBlocked;
            //Finish by unblocking the character.
            await Client.Send(new C_REMOVE_BLOCKED_USER {UserName = characterName}, token);
            return isOnline;
        }
    }
}