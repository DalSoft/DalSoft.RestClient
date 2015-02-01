using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace DalSoft.RestClient
{
    internal sealed class HttpClientWrapper : IHttpClientWrapper
    {
        public IDictionary<string, string> DefaultRequestHeaders { get; set; }
        public const string JsonContentType = "application/json";
        private readonly HttpClient _httpClient;

        public HttpClientWrapper() : this(new HttpClient(), new Dictionary<string, string>()) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders) : this(new HttpClient(), defaultRequestHeaders) { }

        internal HttpClientWrapper(HttpClient httpClient, IDictionary<string, string> defaultRequestHeaders)
        {
            DefaultRequestHeaders = defaultRequestHeaders;

            _httpClient = httpClient;

            if (defaultRequestHeaders.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));
            }
            else
            {
                foreach (var header in defaultRequestHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public async Task<HttpResponseMessage> Send(HttpMethod method, string uri, IDictionary<string, string> requestHeaders, object content)
        {
            requestHeaders = requestHeaders ?? new Dictionary<string, string>() { };

            var httpRequestMessage = new HttpRequestMessage(method, new Uri(uri))
            {
                Content = GetContent(content, requestHeaders),
            };

            foreach (var header in DefaultRequestHeaders)
            {
                if (requestHeaders.Any(x => x.Key == header.Key))
                    requestHeaders.Remove(header.Key);

                requestHeaders.Add(header.Key, header.Value);
            }

            foreach (var header in requestHeaders)
            {
                if (httpRequestMessage.Headers.Contains(header.Key))
                    httpRequestMessage.Headers.Remove(header.Key);

                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        private static HttpContent GetContent(object content, IDictionary<string, string> requestHeaders)
        {
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
                httpContent.Headers.Add("Content-Type", JsonContentType);
            }

            return httpContent;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
