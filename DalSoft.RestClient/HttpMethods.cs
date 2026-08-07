using System;
using System.Collections.Generic;

namespace DalSoft.RestClient
{
    internal static class HttpMethods
    {
        private static readonly HashSet<string> ImmutableHttpMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GET",
            "OPTIONS",
            "HEAD",
            "TRACE"
        };

        private static readonly HashSet<string> MutableHttpMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "POST",
            "PUT",
            "PATCH",
            "MERGE",
            "DELETE"
        };

        internal static bool IsMutableHttpMethod(this string httpMethod)
        {
            return MutableHttpMethods.Contains(httpMethod);
        }

        internal static bool IsHttpMethod(this string httpMethod)
        {
            return IsImmutableHttpMethod(httpMethod) || IsMutableHttpMethod(httpMethod);
        }

        internal static bool IsImmutableHttpMethod(this string httpMethod)
        {
            return ImmutableHttpMethods.Contains(httpMethod);
        }
    }
}
