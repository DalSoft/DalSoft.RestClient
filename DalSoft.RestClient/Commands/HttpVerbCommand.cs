using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Commands
{
    internal class HttpVerbCommand : Command
    {
        internal override bool IsCommandFor(string method, object[] args)
        {
            return method.IsHttpVerb();
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

            var httpResponseMessage = await memberAccessWrapper.HttpClientWrapper.Send
            (
                new HttpMethod(httpMethodString.ToUpperInvariant()), uri, requestHeaders, httpContent
            ).ConfigureAwait(false);

            return new RestClientResponseObject(httpResponseMessage);
        }

        private static Uri ParseUri(string httpMethod, string currentUri, object[] args)
        {
            if (args.Length > 0 && httpMethod.IsImmutableVerb())
            {
                ResourceCommand.ValidateResourceArgs(args);
                currentUri += "/" + args[0];
            }

            if (currentUri.EndsWith("/"))
                currentUri = currentUri.TrimEnd("/".ToCharArray());

            Uri uri;
            if (!Uri.TryCreate(currentUri, UriKind.Absolute, out uri))
                throw new ArgumentException($"{currentUri} is not a valid Absolute Uri");

            return uri;
        }

        internal static IDictionary<string, string> ParseRequestHeaders(object[] args)
        {
            IDictionary<string, string> requestHeaders = new Dictionary<string, string> { };

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

            if (httpMethod.IsImmutableVerb())
                return null;

            return args[0];
        }
    }
}
