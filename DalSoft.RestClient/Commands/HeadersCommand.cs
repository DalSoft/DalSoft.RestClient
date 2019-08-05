﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            if (args[0] is IDictionary<string, string> headers)
                AddHeaders(next, headers);
            else
                AddHeaders(next, new Headers(args[0]));

            return new MemberAccessWrapper
            (
                next.HttpClientWrapper,
                next.BaseUri,
                next.GetRelativeUri(),
                next.Headers
            );
        }

        private static void AddHeaders(MemberAccessWrapper next, IDictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                if (next.Headers.ContainsKey(header.Key))
                    next.Headers[header.Key] = header.Value;
                else
                    next.Headers.Add(header.Key, header.Value);
            }
        }

        protected override void Validate(object[] args)
        {
            if (args == null || args.Length != 1)
                throw new ArgumentException("Headers must have one not null argument only");

            if (args[0] is IDictionary<string, string>)
                return;

            Headers.ValidateAnonymousObjectRepresentingHeaders(args[0]);
        }
    }
}