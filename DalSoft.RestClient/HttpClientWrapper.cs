﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;

namespace DalSoft.RestClient
{
    internal sealed class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        
        public IReadOnlyDictionary<string, string> DefaultRequestHeaders { get; set; } //ToDo: really this should be IDictionary<string, IEnumerable<string>>
        
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

            defaultRequestHeaders = defaultRequestHeaders ?? new Dictionary<string, string>();
            DefaultRequestHeaders = new ReadOnlyDictionary<string, string>(defaultRequestHeaders);
        }

        public async Task<HttpResponseMessage> Send(HttpMethod method, Uri uri, IDictionary<string, string> requestHeaders, object content)
        {
            requestHeaders = requestHeaders ?? new Dictionary<string, string>();

            foreach (var defaultHeader in DefaultRequestHeaders) 
            {
                if (!requestHeaders.ContainsKey(defaultHeader.Key)) //Only add the defaultHeader if it's not in passed in the requestHeaders argument allowing us to override default headers
                    requestHeaders.Add(defaultHeader.Key, defaultHeader.Value);
            }
            
            var httpRequestMessage = new HttpRequestMessage(method, uri);
            httpRequestMessage.Headers.Clear(); //Clear the defaults we want to control all the headers

            foreach (var header in requestHeaders)
            {
                /* If we try and add the Content-Type header to The HttpRequest we get the following exception 
                 * System.InvalidOperationException : Misused header name. Make sure request headers are used with HttpRequestMessage, response headers with HttpResponseMessage, 
                 * and content headers with HttpContent objects.
                 * So add it to the StateBag so that the Handlers can set the Content-Type header when building the HttpContent object */

                if (header.Key.ToLower() == "content-type")
                    httpRequestMessage.SetContentType(header.Value);
                else
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
            }

            httpRequestMessage.SetContent(content);

            return await _httpClient.SendAsync(httpRequestMessage);
        }
        
        private static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<HttpMessageHandler> handlers)
        {
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
