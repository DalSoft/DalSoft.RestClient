using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    public class UnitTestHandler : DelegatingHandler
    {
        private Stream _streamContent;

        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        
        public UnitTestHandler() : this((Action<HttpRequestMessage>) null) { }

        public UnitTestHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler ?? (request => new HttpResponseMessage()) ;
        }

        public UnitTestHandler(Action<HttpRequestMessage> handler)
        {
            handler = handler ?? (request => { });

            _handler = request =>
            {
                handler(request);
                return new HttpResponseMessage();
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response =  _handler(request);

            response.RequestMessage = response.RequestMessage ?? request ?? new HttpRequestMessage();
            response.Content= response.Content?? new StringContent(string.Empty);

            if (_streamContent == null)
            {
                _streamContent = new MemoryStream();
                await response.Content.CopyToAsync(_streamContent);
            }

            _streamContent.Position = 0;

            var localStream = new MemoryStream();
            await _streamContent.CopyToAsync(localStream);

            localStream.Position = 0;

            response.Content = new StreamContent(localStream); //Allow content to be re-used in tests

            return await Task.FromResult(response).ConfigureAwait(false);
        }
    }
}
