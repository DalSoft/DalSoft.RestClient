using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
                AddHeaders(next, headers);
            else
                AddHeaders(next, args[0].GetType().GetProperties()
                    .Where(_ => _.GetValue(args[0]) != null)
                    .ToDictionary(_=> FormatHeaderName(_.Name),  _=>_.GetValue(args[0]).ToString()));

            return new MemberAccessWrapper
            (
                next.HttpClientWrapper,
                next.BaseUri,
                next.GetRelativeUri(),
                next.Headers
            );
        }

        private static string FormatHeaderName(string propertyName)
        {
            return Regex.Replace(propertyName, @"(\p{Ll})(\p{Lu})", "$1-$2");
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

            var incorrectTypeException = new ArgumentException("Headers must be Dictionary<string, string> or a simple object representing the Headers new { ContentType = \"application/json\", Accept = \"application/json\" }"); ;

            if (args[0] is IEnumerable)
                throw incorrectTypeException;

            if (!(args[0].GetType().GetTypeInfo().IsClass))
                throw incorrectTypeException;

            if (args[0].GetType().GetProperties().Any(_ => _.GetValue(args[0]).GetType() != typeof(string)))
                throw incorrectTypeException;
        }
    }
}