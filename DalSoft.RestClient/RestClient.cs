using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;

namespace DalSoft.RestClient
{
    /// <summary>RestClient is a conventions based dyanmic Rest Client by Darran Jones</summary>
    public class RestClient : DynamicObject, IDisposable
    {
        internal readonly IHttpClientWrapper HttpClientWrapper;
        public IDictionary<string, string> DefaultRequestHeaders => HttpClientWrapper.DefaultRequestHeaders;
        public string BaseUri { get; }
        public Config Config { get; private set; }
     
        public RestClient(string baseUri) : this(new HttpClientWrapper(), baseUri) { }
        public RestClient(string baseUri, HttpMessageHandler httpMessageHandler) : this(new HttpClientWrapper(httpMessageHandler), baseUri) { }

        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders) : this(new HttpClientWrapper(defaultRequestHeaders), baseUri) { }
        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders, HttpMessageHandler httpMessageHandler) : this(new HttpClientWrapper(defaultRequestHeaders, httpMessageHandler), baseUri) { }

        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders, HttpMessageHandler httpMessageHandler, Config config) : this(new HttpClientWrapper(defaultRequestHeaders, httpMessageHandler, config), baseUri) { }
        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders, Config config) : this(new HttpClientWrapper(defaultRequestHeaders, config), baseUri) { }
        public RestClient(string baseUri, Config config) : this(new HttpClientWrapper(config), baseUri) { }

        public RestClient(IHttpClientWrapper httpClientWrapper, string baseUri)
        {
            HttpClientWrapper = httpClientWrapper;
            BaseUri = baseUri;
            Config = httpClientWrapper.Config;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, binder.Name);
            return true;
        }

        public void Dispose()
        {
            HttpClientWrapper.Dispose();
        }
    }
}
