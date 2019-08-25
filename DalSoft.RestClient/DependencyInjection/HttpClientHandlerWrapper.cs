using System.Net.Http;

namespace DalSoft.RestClient.DependencyInjection
{
    /// <summary>Used so we a able to find the primary handler, when wanting to add things like cookies using RestClientFactory</summary>
    // ReSharper disable once InheritdocConsiderUsage
    internal class HttpClientHandlerWrapper : HttpClientHandler
    {

    }
}