using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DalSoft.RestClient
{
    /// <summary>RestClient is a conventions based dyanmic Rest Client by Darran Jones</summary>
    public class RestClient : DynamicObject, IDisposable
    {
        private readonly IHttpClientWrapper _httpClientWrapper;

        public IDictionary<string, string> DefaultRequestHeaders
        {
            get { return _httpClientWrapper.DefaultRequestHeaders; }
        }
        
        public string BaseUri { get; private set; }

        public RestClient(string baseUri) : this(new HttpClientWrapper(), baseUri) { }

        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders) : this(new HttpClientWrapper(defaultRequestHeaders), baseUri) { }

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
