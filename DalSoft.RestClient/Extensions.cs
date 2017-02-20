using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient
{
    internal static class Extensions
    {
        private static readonly string[] ImmutableVerbs = new[] {
            HttpMethodEnum.GET.ToString(),
            HttpMethodEnum.DELETE.ToString(),
            HttpMethodEnum.OPTIONS.ToString(),
            HttpMethodEnum.HEAD.ToString(),
            HttpMethodEnum.TRACE.ToString()
        };

        private static readonly string[] MutableVerbs = new[] {
            HttpMethodEnum.POST.ToString(),
            HttpMethodEnum.PUT.ToString(),
            HttpMethodEnum.PATCH.ToString()
        };

        public static bool IsHttpVerb(this string httpMethod)
        {
            return IsImmutableVerb(httpMethod) || IsMutableVerb(httpMethod);
        }
        
        public static bool IsImmutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return ImmutableVerbs.Any(x => x == httpMethod);
        }
        
        public static bool IsMutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpperInvariant();
            return MutableVerbs.Any(x => x == httpMethod);
        }

        public static Uri GetUri(string httpMethod, string currentUri, object[] args)
        {
            if (args.Length > 0 && httpMethod.IsImmutableVerb())
            {
                args.ValidateResourceArgs();
                currentUri += "/" + args[0];
            }

            if (currentUri.EndsWith("/"))
                currentUri = currentUri.TrimEnd("/".ToCharArray());
            
            Uri uri;
            if (!Uri.TryCreate(currentUri, UriKind.Absolute, out uri))
                throw new ArgumentException($"{currentUri} is not a valid Uri");
            
            return uri;
        }

        public static void ValidateHttpVerbArgs(this object[] args)
        {
            if (args.Length == 0)
                return;

            if (args.Length > 2)
                throw new ArgumentException("You can only pass two arguments, first is the resource or object to be serialized, second is the RequestHeaders");

            if (args.Length == 2 && (args[1] as IDictionary<string, string>) == null)
                throw new ArgumentException("Second argument must be a Dictionary of RequestHeaders");
        }

        public static void ValidateResourceArgs(this object[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Please provide a resource");

            if (args[0]==null)
                return;

            if (!args[0].GetType().GetTypeInfo().IsPrimitive && args[0].GetType() != typeof(string))
                throw new ArgumentException("Resource must be a primitive type or a string");
        }

        public static IDictionary<string, string> GetRequestHeaders(this object[] args)
        {
            IDictionary<string, string> requestHeaders = new Dictionary<string, string> { };
            if (args.Length == 2)
                requestHeaders = (IDictionary<string, string>)args[1];

            return requestHeaders;
        }

        public static object ParseContent(string httpMethod, object[] args)
        {
            if (args.Length == 0)
                return null;

            if (args[0] == null)
                return null;

            if (IsImmutableVerb(httpMethod))
                return null;
            
            if (!args[0].GetType().GetTypeInfo().IsClass || args[0] is string)
                throw new ArgumentException("Please provide a class to be serialized to the request body for example new { hello = \"world\" }");

            return args[0];
        }

        public static bool TryParseJson(this string json, out object result, Type type = null)
        {
            type = type ?? typeof(object);
            try
            {
                result = JsonConvert.DeserializeObject(json, type);
                return true;
            }
            catch (Exception ex)
            {
                result = ex;
                return false; //Eat invalid json  
            }
        }

        public static object WrapJToken(this JToken jToken)
        {
            object result = null;

            //JObject
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                result = new RestClientResponseObject(jObject);
            }

            //JValue
            var jValue = jToken as JValue;
            if (jValue != null)
            {
                result = jValue.Value;
            }

            //JArray
            var jArray = jToken as JArray;
            if (jArray != null)
            {
                result = jArray.Select(WrapJToken).ToArray();
            }

            return result;
        }

        public static object GetContent(this HttpRequestMessage request)
        {
            return request.Properties[Config.Contentkey];
        }
    }
}
