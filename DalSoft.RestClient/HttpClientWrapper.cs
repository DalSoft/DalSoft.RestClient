using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace DalSoft.RestClient
{
    internal sealed class HttpClientWrapper : IHttpClientWrapper
    {
        internal readonly HttpClient HttpClient;
        
        public IReadOnlyDictionary<string, string> DefaultRequestHeaders { get; set; } //ToDo: really this should be IDictionary<string, IEnumerable<string>>
        
        public HttpClientWrapper() : this(null, (Config)null) { }

        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders) : this(defaultRequestHeaders, null) { }

        public HttpClientWrapper(Config config) : this(null, config) { }
        
        public HttpClientWrapper(IDictionary<string, string> defaultRequestHeaders, Config config)
        {
            config = config ?? new Config();

            HttpClient = new HttpClient(config.CreatePipeline())
            {
                Timeout = config.Timeout,
                MaxResponseContentBufferSize = config.MaxResponseContentBufferSize
            };

            DefaultRequestHeaders = new ReadOnlyDictionary<string, string>(defaultRequestHeaders ?? new Dictionary<string, string>());
        }

        //Called by RestClientFactory
        public HttpClientWrapper(HttpClient httpClient, IDictionary<string, string> defaultRequestHeaders)
        {  
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            DefaultRequestHeaders = new ReadOnlyDictionary<string, string>(defaultRequestHeaders ?? new Dictionary<string, string>());
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

            return await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        }
        
        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
