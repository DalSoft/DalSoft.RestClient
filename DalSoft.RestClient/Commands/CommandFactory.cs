using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DalSoft.RestClient.Commands
{
    internal class CommandFactory
    {
        private static readonly ReadOnlyCollection<Command> Commands = new ReadOnlyCollection<Command>(new List<Command>
        {
            new HeadersCommand(),
            new EscapedResourceCommand(),
            new QueryCommand(),
            new HttpMethodCommand()
        });

        private static readonly Command DefaultCommand = new ResourceCommand();

        internal static Command GetCommandFor(string method, object[] args)
        {
            //FirstOrDefault to make sure that the default commands e.g. Get, Query Etc.
            var command = Commands.FirstOrDefault(_ => _.IsCommandFor(method, args));

            if (command == null && DefaultCommand.IsCommandFor(method, args))
            {
                return DefaultCommand; //Resource is our default it does the chaining .api.v1.users.etc
            }

            return command;
        }
    }
}