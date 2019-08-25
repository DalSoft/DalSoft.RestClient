using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    // Can't be passed directly to RestClient ctor as requires pipeline interaction with HttpClientHandler. Therefore can only be added via a UseHandler pipeline extension that can set HttpClientHandler properties before creation
    internal class CookieHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.TryGetHttpClientHandler(out var httpClientHandlerWrapper))
                request.SetCookieContainer(httpClientHandlerWrapper.CookieContainer);
            
            return base.SendAsync(request, cancellationToken);
        }       
    }
}
