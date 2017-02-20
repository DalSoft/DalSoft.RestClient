using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    public class UnitTestHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly Action<HttpRequestMessage> _onRequest;
        private readonly Stream _streamContent;

        public UnitTestHandler() : this(new HttpResponseMessage(), request => { }) { }

        public UnitTestHandler(HttpResponseMessage httpResponseMessage) : this(httpResponseMessage, request => { }) { }

        public UnitTestHandler(Action<HttpRequestMessage> onRequest) : this(new HttpResponseMessage(), onRequest) { }

        public UnitTestHandler(HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage> onRequest)
        {
            _httpResponseMessage = httpResponseMessage;
            _streamContent = new MemoryStream();

            httpResponseMessage.Content = httpResponseMessage.Content ?? new StreamContent(_streamContent);
            httpResponseMessage.Content.CopyToAsync(_streamContent);
            
            _onRequest = onRequest;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _httpResponseMessage.RequestMessage = _httpResponseMessage.RequestMessage ?? request ?? new HttpRequestMessage();

            _onRequest(request);

            _streamContent.Position = 0;

            var localStream = new MemoryStream();
            await _streamContent.CopyToAsync(localStream);

            localStream.Position = 0;

            _httpResponseMessage.Content = new StreamContent(localStream); //Allow content to be re-used in tests
     
            return await Task.FromResult(_httpResponseMessage).ConfigureAwait(false);
        }
    }
}
