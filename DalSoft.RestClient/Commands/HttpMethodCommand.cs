using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Commands
{
    internal class HttpMethodCommand : Command
    {
        private static readonly char[] Slash = { '/' };

        private static readonly Dictionary<string, HttpMethod> CachedHttpMethods = new Dictionary<string, HttpMethod>(StringComparer.OrdinalIgnoreCase)
        {
            { "GET", HttpMethod.Get },
            { "POST", HttpMethod.Post },
            { "PUT", HttpMethod.Put },
            { "DELETE", HttpMethod.Delete },
            { "HEAD", HttpMethod.Head },
            { "OPTIONS", HttpMethod.Options },
            { "TRACE", HttpMethod.Trace },
            { "PATCH", new HttpMethod("PATCH") },
            { "MERGE", new HttpMethod("MERGE") }
        };

        internal override bool IsCommandFor(string method, object[] args)
        {
            return method.IsHttpMethod();
        }

        internal override bool IsAsync()
        {
            return true;
        }

        protected override void Validate(object[] args)
        {
            if (args.Length == 0)
                return;

            if (args.Length > 2)
                throw new ArgumentException("You can only pass two arguments, first is the resource or object to be serialized, second is the RequestHeaders");

            if (args.Length == 2 && (args[1] as IDictionary<string, string>) == null)
                throw new ArgumentException("Second argument must be a Dictionary of RequestHeaders");
        }

        protected override async Task<object> HandleAsync(object[] args, MemberAccessWrapper memberAccessWrapper)
        {
            var httpMethodString = memberAccessWrapper.GetLastCall();
            var uri = ParseUri(httpMethodString, memberAccessWrapper.ToString(), args);
            var requestHeaders = ParseRequestHeaders(args);
            var httpContent = ParseContent(httpMethodString, args);

            foreach (var header in memberAccessWrapper.Headers) 
            {
                if (!requestHeaders.ContainsKey(header.Key)) //Only add the header if it's not in passed in the verb's header argument allowing us to override headers set via the Headers() method
                    requestHeaders.Add(header.Key, header.Value);
            }

            var httpResponseMessage = await memberAccessWrapper.HttpClientWrapper.Send(CachedHttpMethods[httpMethodString], uri, requestHeaders, httpContent)
                .ConfigureAwait(false);

            //Don't dispose the content here, the response is handed to the caller who can cast to HttpResponseMessage - reading buffers so the content can be re-read
            var content = httpResponseMessage.Content;

            if (content == null)
                return new RestClientResponseObject(httpResponseMessage, string.Empty);

            var charSet = content.Headers.ContentType?.CharSet;
            if (charSet == null || string.Equals(charSet, "utf-8", StringComparison.OrdinalIgnoreCase))
            {
                var utf8Body = await content.ReadAsByteArrayAsync().ConfigureAwait(false); //Skip the utf-16 string, the serializer can work on utf-8 bytes and the string is decoded lazily
                return new RestClientResponseObject(httpResponseMessage, utf8Body);
            }

            var responseString = await content.ReadAsStringAsync().ConfigureAwait(false); //Non utf-8 charset, let HttpContent do the decoding
            return new RestClientResponseObject(httpResponseMessage, responseString);
        }

        private static Uri ParseUri(string httpMethod, string currentUri, object[] args)
        {
            if (args.Length > 0 && httpMethod.IsImmutableHttpMethod())
            {
                ResourceCommand.ValidateResourceArgs(args);
                currentUri += "/" + args[0];
            }

            if (currentUri.EndsWith("/"))
                currentUri = currentUri.TrimEnd(Slash);

            if (!Uri.TryCreate(currentUri, UriKind.Absolute, out var uri))
                throw new UriFormatException($"{currentUri} is not a valid Absolute Uri");

            return uri;
        }

        internal static IDictionary<string, string> ParseRequestHeaders(object[] args)
        {
            IDictionary<string, string> requestHeaders = new Dictionary<string, string>();

            if (args.Length == 2)
                requestHeaders = (IDictionary<string, string>)args[1];

            return requestHeaders;
        }

        internal static object ParseContent(string httpMethod, object[] args)
        {
            if (args.Length == 0)
                return null;

            if (args[0] == null)
                return null;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (httpMethod.IsImmutableHttpMethod())
                return null;

            return args[0];
        }
    }
}
