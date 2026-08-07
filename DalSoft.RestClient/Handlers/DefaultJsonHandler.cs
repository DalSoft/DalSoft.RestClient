using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace DalSoft.RestClient.Handlers
{
    internal class DefaultJsonHandler : DelegatingHandler
    {
        private readonly Config _config;

        public DefaultJsonHandler(Config config)
        {
            _config = config ?? new Config();
        }        
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetConfig(_config);
            
            if (_config.UseDefaultHandlers)
            {
                request.Content = GetContent(request);
                
                if (!request.Headers.Accept.Any())
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Config.JsonMediaType));

                request.ExpectJsonResponse(true);
            }
            
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static HttpContent GetContent(HttpRequestMessage request)
        {
            var contentType = request.GetContentType();

            if (!IsJsonContentType(contentType))
                return null;

            var content = request.GetContent();

            if (content == null)
                return null;

            if (!content.GetType().GetTypeInfo().IsClass)
                throw new ArgumentException("Please provide a object or string to be serialized to the request body for example .Post(new { hello = \"world\" })");

            HttpContent httpContent;
            if (content is string s)
                httpContent = new StringContent(s);
            else if (request.GetConfig().JsonSerializer.TrySerializeToUtf8Bytes(content, out var utf8Json))
                httpContent = new ByteArrayContent(utf8Json); //Skips the intermediate string
            else
                httpContent = new StringContent(request.GetConfig().JsonSerializer.Serialize(content));

            httpContent.Headers.Clear(); //Clear the defaults we want to control all the headers

            httpContent.Headers.Add("Content-Type", contentType ?? Config.JsonMediaType); //Default to Json Content-Type

            return httpContent;
        }

        private static bool IsJsonContentType(string contentType)
        {
            switch (contentType)
            {
                case null: //Default to Json Content-Type
                case "application/json":
                case "text/json":
                case "application/json-patch+json":
                    return true;
                default:
                    return false;
            }
        }
    }
}
