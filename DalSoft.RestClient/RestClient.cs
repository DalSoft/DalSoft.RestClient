using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;

namespace DalSoft.RestClient
{
    /// <summary>RestClient is a conventions based dyanmic Rest Client by Darran Jones</summary>
    public class RestClient : DynamicObject, IDisposable
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        public IDictionary<string, string> DefaultRequestHeaders => _httpClientWrapper.DefaultRequestHeaders;
        public string BaseUri { get; }
        public TimeSpan? Timeout
        {
            get { return _httpClientWrapper.Timeout; }
            set { _httpClientWrapper.Timeout = value; }
        }

        public RestClient(string baseUri) : this(new HttpClientWrapper(), baseUri) { }
        public RestClient(string baseUri, HttpMessageHandler httpMessageHandler) : this(new HttpClientWrapper(httpMessageHandler), baseUri) { }

        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders) : this(new HttpClientWrapper(defaultRequestHeaders), baseUri) { }
        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders, HttpMessageHandler httpMessageHandler) : this(new HttpClientWrapper(defaultRequestHeaders, httpMessageHandler), baseUri) { }

        public RestClient(IHttpClientWrapper httpClientWrapper, string baseUri)
        {
            _httpClientWrapper = httpClientWrapper;
            BaseUri = baseUri;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(_httpClientWrapper, BaseUri, binder.Name);
            return true;
        }

        public void Dispose()
        {
            _httpClientWrapper.Dispose();
        }
    }
}
