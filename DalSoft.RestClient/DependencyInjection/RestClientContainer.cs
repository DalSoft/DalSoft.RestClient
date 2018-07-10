using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DalSoft.RestClient.DependencyInjection
{
    internal class RestClientContainer
    {
        public Func<RestClient> RestClient { get; }
        public string Name { get; }

        public RestClientContainer(IServiceProvider serviceProvider, string name, string baseUri, Headers headers)
        {
            Name = name;
            
            RestClient = () =>
            {
                var httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient(name);
                return new RestClient(new HttpClientWrapper(httpClient, headers), baseUri);
            };
        }
    }
}
