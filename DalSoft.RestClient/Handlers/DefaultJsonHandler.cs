using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            if (!IsJsonContentType(request))
                return null;

            var content = request.GetContent();

            if (content == null)
                return null;

            if (!content.GetType().GetTypeInfo().IsClass || content is string)
                throw new ArgumentException("Please provide a class to be serialized to the request body for example new { hello = \"world\" }");

            var httpContent = new StringContent(JsonConvert.SerializeObject(content, request.GetConfig().JsonSerializerSettings));

            httpContent.Headers.Clear(); //Clear the defaults we want to control all the headers

            httpContent.Headers.Add("Content-Type", request.GetContentType() ?? Config.JsonMediaType); //Default to Json Content-Type

            return httpContent;
        }

        private static bool IsJsonContentType(HttpRequestMessage request)
        {
            return
                request.GetContentType() == null || //Default to Json Content-Type
                request.GetContentType() == "application/json" ||
                request.GetContentType() == "text/json" ||
                request.GetContentType() == "application/json-patch+json";
        }
    }
}
