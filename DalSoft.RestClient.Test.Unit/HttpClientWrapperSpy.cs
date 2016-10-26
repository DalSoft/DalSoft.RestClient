using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Test.Unit
{
    internal class HttpClientWrapperSpy : IHttpClientWrapper
    {
        public IDictionary<string, string> DefaultRequestHeaders
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public HttpMethod Method { get; private set; }
        public Uri Uri { get; private set; }
        public IDictionary<string, string> RequestHeaders { get; private set; }
        public object Content { get; private set; }

        public Task<HttpResponseMessage> Send(HttpMethod method, Uri uri, IDictionary<string, string> requestHeaders, object content)
        {
            Method = method;
            Uri = uri;
            RequestHeaders = requestHeaders;
            Content = content;

            return Task.FromResult(new HttpResponseMessage()
            {
                RequestMessage = new HttpRequestMessage(method, uri)
            });
        }
    }
}
