using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lotus.Dispatching.Attributes;

namespace SampleBot
{
    //Module responsible of dispatching commands.
    internal class CommandsModule : MyModuleBase
    {
        //Command to demonstrate argument type conversion. While Dispatcher.Execute expects an array of objects for arguments,
        //it converts them to the types in the command handler's signature using casting or a converter type specified
        //in the dispatcher's configuration (in this case, ObjectConverter).
        [Command]
        public void Test(string a, int b, bool c) => Console.WriteLine($"{a} {b} {c}");

        //When a string is routed through the dispatcher, this method will parse it and let the dispatcher execute the
        //command specified by the first word and eventual arguments.
        [Listener]
        public async Task Execute(string value, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var (command, arguments) = GetCommandAndArguments(value);
            try
            {
                await Dispatcher.Execute(command, arguments, 0, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                throw new MyTeraException($"There is no command named: {command}");
            }
        }

        //To allow whitespace inside of arguments, surround them with quotes.
        private static (string, object[]) GetCommandAndArguments(string value)
        {
            var tokenGroups = value.Split('"');
            if (tokenGroups.Length % 2 != 1) throw new Exception("The command contains unmatched quotes.");
            //Split only words at even indexes (not inside quote blocks).
            var tokens = tokenGroups.SelectMany((token, index) => index % 2 > 0
                    ? new[] {token}
                    : token.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                .ToList();
            var command = tokens.First();
            var arguments = tokens.Skip(1).Cast<object>().ToArray();
            return (command, arguments);
        }
    }
}