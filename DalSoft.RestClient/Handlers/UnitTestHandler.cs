using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    public class UnitTestHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly Action<HttpRequestMessage> _onRequest;

        public UnitTestHandler() : this(new HttpResponseMessage(), request => { }) { }

        public UnitTestHandler(HttpResponseMessage httpResponseMessage) : this(httpResponseMessage, request => { }) { }

        public UnitTestHandler(Action<HttpRequestMessage> onRequest) : this(new HttpResponseMessage(), onRequest) { }

        public UnitTestHandler(HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage> onRequest)
        {
            _httpResponseMessage = httpResponseMessage;
            _onRequest = onRequest;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _httpResponseMessage.RequestMessage = _httpResponseMessage.RequestMessage ?? request ?? new HttpRequestMessage();

            _onRequest(request);

            return await Task.FromResult(_httpResponseMessage).ConfigureAwait(false);
        }
    }
}
