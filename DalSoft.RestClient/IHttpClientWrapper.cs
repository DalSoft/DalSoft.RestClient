using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DalSoft.RestClient
{
    public interface IHttpClientWrapper : IDisposable
    {
        Task<HttpResponseMessage> Send(HttpMethod method, string uri, IDictionary<string, string> requestHeaders, object content);
        IDictionary<string, string> DefaultRequestHeaders { get; set; }
    }
}