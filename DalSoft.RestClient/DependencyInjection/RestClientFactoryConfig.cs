using System;
using DalSoft.RestClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DalSoft.RestClient.DependencyInjection
{
    public class RestClientFactoryConfig
    {
        public IHttpClientBuilder HttpClientBuilder { get; }
        public bool UseDefaultHandlers { get; internal set; }
        public JsonSerializerSettings JsonSerializerSettings { get; internal set; }

        public RestClientFactoryConfig(IHttpClientBuilder httpClientBuilder)
        {
            HttpClientBuilder = httpClientBuilder ?? throw new ArgumentNullException(nameof(httpClientBuilder));
            UseDefaultHandlers = true;

            HttpClientBuilder.AddHttpMessageHandler(() => new DefaultJsonHandler(new Config
            {
                UseDefaultHandlers = UseDefaultHandlers,
                JsonSerializerSettings = JsonSerializerSettings
            }));
        }
    }
}
