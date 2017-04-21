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
            request = await CloneRequestContent(request).ConfigureAwait(false);

            var response = _handler(request) ?? new HttpResponseMessage();

            response = await CloneResponseContent(response).ConfigureAwait(false);

            response.RequestMessage = response.RequestMessage ?? request ?? new HttpRequestMessage();

            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private static async Task<HttpRequestMessage> CloneRequestContent(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Content == null)
                return await Task.FromResult(httpRequestMessage).ConfigureAwait(false);

            var localStream = new MemoryStream();
            await httpRequestMessage.Content.CopyToAsync(localStream);  //Allow content to be re-read in tests

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

        private static async Task<HttpResponseMessage> CloneResponseContent(HttpResponseMessage httpResponseMessage)
        {
            httpResponseMessage.Content = httpResponseMessage.Content ?? new StringContent(string.Empty);

            var localStream = new MemoryStream();
            await httpResponseMessage.Content.CopyToAsync(localStream); //Allow content to be re-read in tests

            localStream.Position = 0;

            var response = new HttpResponseMessage
            {
                Content = new StreamContent(localStream),
                StatusCode = httpResponseMessage.StatusCode,
                ReasonPhrase = httpResponseMessage.ReasonPhrase,
                Version = httpResponseMessage.Version
            };

            httpResponseMessage.Content.Headers.ToList().ForEach(contentHeader => { response.Content.Headers.Add(contentHeader.Key, contentHeader.Value); });
            httpResponseMessage.Headers.ToList().ForEach(header => { response.Headers.Add(header.Key, header.Value); });
         
            return response;
        }
    }
}
