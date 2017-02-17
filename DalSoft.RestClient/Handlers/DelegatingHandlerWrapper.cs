using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    public class DelegatingHandlerWrapper : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> _handler;

        public DelegatingHandlerWrapper(Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> handler)
        {
            _handler = handler ?? ((request, token, next) => base.SendAsync(request, token));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _handler(request, cancellationToken, (message, token) => base.SendAsync(request, cancellationToken));
        }
    }
}