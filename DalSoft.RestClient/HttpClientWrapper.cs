using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;


namespace DalSoft.RestClient
{
    internal sealed class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        
        public IDictionary<string, string> DefaultRequestHeaders { get; set; } //ToDo: really this should be IDictionary<string, IEnumerable<string>>
        
        public HttpClientWrapper() : this(null, null) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders) : this(defaultRequestHeaders, null) { }

        public HttpClientWrapper(Config config) : this(null, config) { }
        
        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders, Config config)
        {
            config = config ?? new Config();

            _httpClient = new HttpClient(CreatePipeline(config.Pipeline.OfType<HttpClientHandler>().SingleOrDefault() ?? new HttpClientHandler(), 
                                                        config.Pipeline.Except(config.Pipeline.OfType<HttpClientHandler>())))
            {
                Timeout = config.Timeout,
                MaxResponseContentBufferSize = config.MaxResponseContentBufferSize
            };

            DefaultRequestHeaders = defaultRequestHeaders ?? new Dictionary<string, string>();
            
            foreach (var header in DefaultRequestHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        public async Task<HttpResponseMessage> Send(HttpMethod method, Uri uri, IDictionary<string, string> requestHeaders, object content)
        {
            requestHeaders = requestHeaders ?? new Dictionary<string, string>() { };

            var httpRequestMessage = new HttpRequestMessage(method, uri);
            
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

            httpRequestMessage.Properties.Add(Config.Contentkey, content);

            return await _httpClient.SendAsync(httpRequestMessage);
        }
        
        private static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<HttpMessageHandler> handlers)
        {
            if (innerHandler == null)
                throw new ArgumentNullException(nameof(innerHandler));
            if (handlers == null)
                return innerHandler;

            var httpMessageHandler = innerHandler;

            foreach (var handler in handlers.Reverse())
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handlers), "Delegating Handler Array Contains Null Item");

                var delegatingHandler = handler as DelegatingHandler;

                if (delegatingHandler == null)
                    delegatingHandler = new HttpMessageHandlerToDelegatingHandler(handler);
                else
                {
                    if (delegatingHandler.InnerHandler != null)
                        throw new ArgumentException("Delegating Handler Array Has Non Null Inner Handler", nameof(handlers));
                    delegatingHandler.InnerHandler = httpMessageHandler;
                }

                httpMessageHandler = delegatingHandler;
            }
            return httpMessageHandler;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
