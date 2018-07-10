using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DalSoft.RestClient
{
    public sealed class Headers : Dictionary<string, string>
    {
        public Headers() { }

        public Headers(object anonymousObjectRepresentingHeaders)
        {
            if (anonymousObjectRepresentingHeaders == null)
                throw new ArgumentNullException(nameof(anonymousObjectRepresentingHeaders));

            foreach (var item in anonymousObjectRepresentingHeaders.GetType().GetProperties().Where(_ => _.GetValue(anonymousObjectRepresentingHeaders) != null))
            {
                Add(FormatHeaderName(item.Name),  item.GetValue(anonymousObjectRepresentingHeaders).ToString());
            }
        }

        private static string FormatHeaderName(string propertyName)
        {
            return Regex.Replace(propertyName, @"(\p{Ll})(\p{Lu})", "$1-$2");
        }

        internal static void VaildateAnonymousObjectRepresentingHeaders(object anonymousObjectRepresentingHeaders)
        {
            var incorrectTypeException = new ArgumentException("Headers must be Dictionary<string, string> or a simple object representing the Headers new { ContentType = \"application/json\", Accept = \"application/json\" }"); ;

            if (anonymousObjectRepresentingHeaders is IEnumerable)
                throw incorrectTypeException;

            if (!(anonymousObjectRepresentingHeaders.GetType().GetTypeInfo().IsClass))
                throw incorrectTypeException;

            if (anonymousObjectRepresentingHeaders.GetType().GetProperties().Any(_ => _.GetValue(anonymousObjectRepresentingHeaders).GetType() != typeof(string)))
                throw incorrectTypeException;
        }
    }
}