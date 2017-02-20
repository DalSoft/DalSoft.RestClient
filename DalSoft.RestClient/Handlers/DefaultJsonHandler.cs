using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DalSoft.RestClient.Handlers
{
    internal class DefaultJsonHandler : DelegatingHandler
    {
        private readonly Config _config;

        public DefaultJsonHandler(Config config)
        {
            _config = config;
        }        
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_config.UseDefaultHandlers)
            {
                request.Content = GetContent(request);
                
                if (!request.Headers.Accept.Any())
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Config.JsonContentType));
            }
            
            return await base.SendAsync(request, cancellationToken);
        }

        private static HttpContent GetContent(HttpRequestMessage request)
        {
            var requestHeaders = request.Headers;
            var content = request.GetContent();            

            if (content == null)
                return null;

            var httpContent = new StringContent(JsonConvert.SerializeObject(content));
            
            if (requestHeaders.Any(x => x.Key == "Content-Type"))
            {
                var contentType = requestHeaders.SingleOrDefault(x => x.Key == "Content-Type");
                httpContent.Headers.Remove("Content-Type");
                httpContent.Headers.Add(contentType.Key, contentType.Value);
                requestHeaders.Remove("Content-Type"); //Remove because HttpClient requires the Content-Type to be attached to HttpContent
            }
            else
            {
                httpContent.Headers.Remove("Content-Type");
                httpContent.Headers.Add("Content-Type", Config.JsonContentType);
            }

            return httpContent;
        }
    }
}
