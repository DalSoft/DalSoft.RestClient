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
        private readonly HttpClient _httpClient;
        public const string JsonContentType = "application/json";
        public IDictionary<string, string> DefaultRequestHeaders { get; set; } //ToDo: really this should be IDictionary<string, IEnumerable<string>>
        public Config Config { get; }

        public HttpClientWrapper() : this(new HttpClient(), null, null) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders) : this(new HttpClient(), defaultRequestHeaders, null) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders, HttpMessageHandler httpMessageHandler) : this(new HttpClient(httpMessageHandler ?? new HttpClientHandler()), defaultRequestHeaders, null) { }

        public HttpClientWrapper(HttpMessageHandler httpMessageHandler) : this(new HttpClient(httpMessageHandler ?? new HttpClientHandler()), null, null) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders, HttpMessageHandler httpMessageHandler, Config config) : this(new HttpClient(httpMessageHandler ?? new HttpClientHandler()), defaultRequestHeaders, config) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders, Config config) : this(new HttpClient(), defaultRequestHeaders, config) { }

        public HttpClientWrapper(Config config) : this(new HttpClient(), null, config) { }

        private HttpClientWrapper(HttpClient httpClient, IDictionary<string, string> defaultRequestHeaders, Config config)
        {
            _httpClient = httpClient;

            Config = config ?? new Config();
            
            DefaultRequestHeaders = defaultRequestHeaders ?? new Dictionary<string, string>();

            _httpClient.Timeout = Config?.Timeout ?? _httpClient.Timeout;

            if (DefaultRequestHeaders.All(_ => _.Key.ToLower() != "accept"))
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));
            
            foreach (var header in DefaultRequestHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        public async Task<HttpResponseMessage> Send(HttpMethod method, Uri uri, IDictionary<string, string> requestHeaders, object content)
        {
            
            requestHeaders = requestHeaders ?? new Dictionary<string, string>() { };

            var httpRequestMessage = new HttpRequestMessage(method, uri)
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
