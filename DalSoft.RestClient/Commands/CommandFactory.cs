namespace DalSoft.RestClient.Commands
{
    internal class CommandFactory
    {
        private static readonly Command[] Commands =
        {
            new HeadersCommand(),
            new EscapedResourceCommand(),
            new QueryCommand(),
            new HttpMethodCommand()
        };

        private static readonly Command DefaultCommand = new ResourceCommand();

        internal static Command GetCommandFor(string method, object[] args)
        {
            //Plain for loop to make sure the default commands e.g. Get, Query Etc. are checked first without allocating a closure per invoke
            for (var i = 0; i < Commands.Length; i++)
            {
                if (Commands[i].IsCommandFor(method, args))
                    return Commands[i];
            }

            if (DefaultCommand.IsCommandFor(method, args))
            {
                return DefaultCommand; //Resource is our default it does the chaining .api.v1.users.etc
            }

            return null;
        }
    }
}