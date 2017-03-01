using System.Linq;

namespace DalSoft.RestClient
{
    internal static class HttpVerbs
    {
        private enum HttpMethodEnum
        {
            // ReSharper disable InconsistentNaming
            GET,
            POST,
            PUT,
            PATCH,
            DELETE,
            HEAD,
            OPTIONS,
            TRACE
            // ReSharper restore InconsistentNaming
        }
        
        private static readonly string[] ImmutableVerbs = 
        {
            HttpMethodEnum.GET.ToString(),
            HttpMethodEnum.DELETE.ToString(),
            HttpMethodEnum.OPTIONS.ToString(),
            HttpMethodEnum.HEAD.ToString(),
            HttpMethodEnum.TRACE.ToString()
        };

        private static readonly string[] MutableVerbs = 
        {
            HttpMethodEnum.POST.ToString(),
            HttpMethodEnum.PUT.ToString(),
            HttpMethodEnum.PATCH.ToString()
        };

        private static bool IsMutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return MutableVerbs.Any(x => x == httpMethod);
        }

        internal static bool IsHttpVerb(this string httpMethod)
        {
            return IsImmutableVerb(httpMethod) || IsMutableVerb(httpMethod);
        }

        internal static bool IsImmutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return ImmutableVerbs.Any(x => x == httpMethod);
        }
    }
}
