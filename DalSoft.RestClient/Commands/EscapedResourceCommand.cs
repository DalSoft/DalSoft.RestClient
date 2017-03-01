using System;

namespace DalSoft.RestClient.Commands
{
    internal class EscapedResourceCommand : Command
    {
        internal override bool IsCommandFor(string method, object[] args)
        {
            return method == "Resource";
        }

        protected override object Handle(object[] args, MemberAccessWrapper next)
        {
            return new MemberAccessWrapper
            (
                next.HttpClientWrapper,
                next.BaseUri,
                next.GetRelativeUri() + "/" + args[0]
            );
        }

        protected override void Validate(object[] args)
        {
            ResourceCommand.ValidateResourceArgs(args);

            if (args.Length != 1)
                throw new ArgumentException("Resource can only have one argument");

        }
    }
}