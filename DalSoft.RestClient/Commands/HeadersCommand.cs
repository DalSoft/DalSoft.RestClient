using System;
using System.Collections.Generic;

namespace DalSoft.RestClient.Commands
{
    internal class HeadersCommand : Command
    {
        internal override bool IsCommandFor(string method, object[] args)
        {
            return method == "Headers";
        }

        protected override object Handle(object[] args, MemberAccessWrapper next)
        {
            var headers = args[0] as IDictionary<string, string>;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (next.Headers.ContainsKey(header.Key))
                        next.Headers[header.Key] = header.Value;
                    else
                        next.Headers.Add(header.Key, header.Value);
                }
            }

            return new MemberAccessWrapper
            (
                next.HttpClientWrapper,
                next.BaseUri,
                next.GetRelativeUri(),
                next.Headers
            );
        }

        protected override void Validate(object[] args)
        {
            if (args==null || args.Length != 1)
                throw new ArgumentException("Headers must have one argument that is Dictionary<string, string>");
            
            //TODO: support object new { ContentType = "application" } as well as dictionary
            if (!(args[0] is IDictionary<string, string>))
                throw new ArgumentException("Headers must be Dictionary<string, string>");
        }
    }
}