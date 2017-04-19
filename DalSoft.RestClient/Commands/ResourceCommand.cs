using System;
using System.Reflection;
using static DalSoft.RestClient.Extensions.Object;

namespace DalSoft.RestClient.Commands
{
    internal class ResourceCommand : Command
    {
        internal override bool IsCommandFor(string method, object[] args)
        {
            return args.Length > 0; 
        }

        protected override object Handle(object[] args, MemberAccessWrapper next)
        {
            return new MemberAccessWrapper //Resource is our default it does the chaining .api.v1.users.etc
            (
                next.HttpClientWrapper, 
                next.BaseUri, 
                next.Uri + "/" + args[0],
                next.Headers
            );
        }

        protected override void Validate(object[] args)
        {
            ValidateResourceArgs(args);
        }

        internal static void ValidateResourceArgs(object[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Please provide a resource");

            if (args[0] == null)
                return;

            if (!IsValueTypeOrPrimitiveOrStringOrGuid(args[0].GetType().GetTypeInfo()))
                throw new ArgumentException("Resource must be a primitive type, string or a Guid");
        }
    }
}
