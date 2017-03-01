using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

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
            var pairs = args[0].GetType().GetProperties()
                   .Select(x => GetQueryParamValue(x, args[0])).ToArray();

            var queryString = "?" + string.Join("&", pairs);

            return new MemberAccessWrapper
            (
                next.HttpClientWrapper, 
                next.BaseUri, 
                next.GetRelativeUri() + queryString
            );
        }

        private static string GetQueryParamValue(PropertyInfo propInfo, object obj)
        {
            var paramName = propInfo.Name + "=";

            if (typeof(IEnumerable<object>).IsAssignableFrom(propInfo.PropertyType))
            {
                var values = (propInfo.GetValue(obj, null) as IEnumerable<object>)
                    .Select(v => paramName + WebUtility.UrlEncode(v.ToString()))
                    .ToArray();

                return string.Join("&", values);
            }

            return paramName + WebUtility.UrlEncode(propInfo.GetValue(obj, null).ToString());
        }
    }
}
