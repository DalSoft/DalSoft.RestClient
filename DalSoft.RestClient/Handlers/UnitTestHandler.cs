using System;
using System.IO;
using System.Linq;
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
            request = await CloneRequestContent(request).ConfigureAwait(false); //Allow request content to be re-used in tests

            var response = _handler(request);

            response.RequestMessage = response.RequestMessage ?? request ?? new HttpRequestMessage();

            await CloneResponseContent(response).ConfigureAwait(false); //Allow response content to be re-used in tests

            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private static async Task<HttpRequestMessage> CloneRequestContent(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Content == null)
                return await Task.FromResult(httpRequestMessage).ConfigureAwait(false);

            var localStream = new MemoryStream();
            await httpRequestMessage.Content.CopyToAsync(localStream);

            localStream.Position = 0;

            var request = new HttpRequestMessage
            {
                Content = new StreamContent(localStream),
                Method = httpRequestMessage.Method,
                RequestUri = httpRequestMessage.RequestUri,
                Version = httpRequestMessage.Version
            };

            httpRequestMessage.Content.Headers.ToList().ForEach(contentHeader => { request.Content.Headers.Add(contentHeader.Key, contentHeader.Value); });
            httpRequestMessage.Headers.ToList().ForEach(header => { request.Headers.Add(header.Key, header.Value); });
            httpRequestMessage.Properties.ToList().ForEach(property => { request.Properties.Add(property.Key, property.Value);});

            return request;
        }

        private async Task CloneResponseContent(HttpResponseMessage response)
        {
            response.Content = response.Content ?? new StringContent(string.Empty);

            if (_streamContent == null)
            {
                _streamContent = new MemoryStream();
                await response.Content.CopyToAsync(_streamContent).ConfigureAwait(false);
            }

            _streamContent.Position = 0;

            var localStream = new MemoryStream();
            await _streamContent.CopyToAsync(localStream).ConfigureAwait(false);

            localStream.Position = 0;

            response.Content = new StreamContent(localStream); //Allow content to be re-used in tests
        }
    }
}
