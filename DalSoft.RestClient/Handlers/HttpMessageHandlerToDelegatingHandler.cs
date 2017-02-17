using System.Net.Http;

namespace DalSoft.RestClient.Handlers
{
    internal class HttpMessageHandlerToDelegatingHandler : DelegatingHandler
    { 
        public HttpMessageHandlerToDelegatingHandler(HttpMessageHandler innerHandler)
        { 
            base.InnerHandler = innerHandler;
        }
    }
}