using System;
using System.Collections.Generic;
using System.Text;
using DalSoft.RestClient.Extensions;
using Object = DalSoft.RestClient.Extensions.Object;

namespace DalSoft.RestClient.Commands
{
    internal class QueryCommand : Command
    {
        internal override bool IsCommandFor(string method, object[] args)
        {
            return method == "Query";
        }
        
        protected override void Validate(object[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Please provide a query");

            if (args.Length != 1)
                throw new ArgumentException("Query has one argument");

            if (args[0].GetType().Namespace != null)
                throw new ArgumentException("Query must be a anonymous type");
        }

        protected override object Handle(object[] args, MemberAccessWrapper next)
        {
           var queryString = ToQueryString(args[0].FlattenObjectToKeyValuePairs<string>(includeThisType: Object.IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime));

            return new MemberAccessWrapper
            (
                next.HttpClientWrapper, 
                next.BaseUri, 
                next.GetRelativeUri() + queryString,
                next.Headers
            );
        }

        private static string ToQueryString(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            var stringBuilder = new StringBuilder();
            foreach (var nameValue in nameValueCollection)
            {
                stringBuilder.Append(stringBuilder.Length == 0 ? '?' : '&');    
                stringBuilder.Append(Encode(nameValue.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(Encode(nameValue.Value));
            }
            return stringBuilder.ToString();
        }

        private static string Encode(string data)
        {
            return string.IsNullOrEmpty(data) ? string.Empty : Uri.EscapeDataString(data).Replace("%20", "+");
        }
    }
}
