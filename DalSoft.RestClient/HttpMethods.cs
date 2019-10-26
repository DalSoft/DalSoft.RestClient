using System.Linq;

namespace DalSoft.RestClient
{
    internal static class HttpMethods
    {
        private enum HttpMethod
        {
            // ReSharper disable InconsistentNaming
            GET,
            POST,
            PUT,
            PATCH,
            MERGE,
            DELETE,
            HEAD,
            OPTIONS,
            TRACE
            // ReSharper restore InconsistentNaming
        }
        
        private static readonly string[] ImmutableHttpMethods = 
        {
            HttpMethod.GET.ToString(),
            HttpMethod.OPTIONS.ToString(),
            HttpMethod.HEAD.ToString(),
            HttpMethod.TRACE.ToString()
        };

        private static readonly string[] MutableHttpMethods = 
        {
            HttpMethod.POST.ToString(),
            HttpMethod.PUT.ToString(),
            HttpMethod.PATCH.ToString(),
            HttpMethod.MERGE.ToString(),
            HttpMethod.DELETE.ToString()
        };

        internal static bool IsMutableHttpMethod(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return MutableHttpMethods.Any(x => x == httpMethod);
        }

        internal static bool IsHttpMethod(this string httpMethod)
        {
            return IsImmutableHttpMethod(httpMethod) || IsMutableHttpMethod(httpMethod);
        }

        internal static bool IsImmutableHttpMethod(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return ImmutableHttpMethods.Any(x => x == httpMethod);
        }
    }
}
