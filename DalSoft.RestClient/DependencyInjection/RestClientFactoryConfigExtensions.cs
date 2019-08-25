using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DalSoft.RestClient.DependencyInjection
{
    public static class RestClientFactoryRestClientFactoryConfigExtensions
    {
        public static RestClientFactoryConfig SetJsonSerializerSettings(this RestClientFactoryConfig config, JsonSerializerSettings jsonSerializerSettings)
        {
            config.JsonSerializerSettings = jsonSerializerSettings;
            return config;
        }
        
        public static RestClientFactoryConfig UseNoDefaultHandlers(this RestClientFactoryConfig config)
        {
            config.UseDefaultHandlers = false;
            return config;
        }

        public static RestClientFactoryConfig UseHandler(this RestClientFactoryConfig config, Func<DelegatingHandler> handler)
        {
            config.HttpClientBuilder.AddHttpMessageHandler(handler);

            return config;
        }
        
        public static RestClientFactoryConfig UseHandler(this RestClientFactoryConfig config, Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> handler)
        {
            DelegatingHandler HandlerFactory() => new DelegatingHandlerWrapper(handler);

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        [Obsolete("Use UseHttpClientHandler(RestClientFactoryConfig, Action<HttpClientHandler>) overload instead")]
        public static RestClientFactoryConfig UseHttpClientHandler(this RestClientFactoryConfig config, Func<HttpClientHandler> handler)
        {
            config.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(handler);

            return config;
        }

        public static RestClientFactoryConfig UseHttpClientHandler(this RestClientFactoryConfig config, Action<HttpClientHandler> httpClientHandlerOptions)
        {
            HttpClientHandler ConfigureHttpClientHandler(IServiceProvider provider) // Delegate called at Factory creation time
            {
                var clientHandler = provider.GetRequiredService<HttpClientHandlerWrapper>();
                httpClientHandlerOptions(clientHandler);

                return clientHandler;
            }

            config.HttpClientBuilder.Services.TryAddTransient<HttpClientHandlerWrapper>();
            config.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(ConfigureHttpClientHandler);

            return config;
        }

        public static RestClientFactoryConfig UseUnitTestHandler(this RestClientFactoryConfig config)
        {
            DelegatingHandler HandlerFactory() => new UnitTestHandler();

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseUnitTestHandler(this RestClientFactoryConfig config, Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            DelegatingHandler HandlerFactory() => new UnitTestHandler(handler);

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseUnitTestHandler(this RestClientFactoryConfig config, Action<HttpRequestMessage> handler)
        {
            DelegatingHandler HandlerFactory() => new UnitTestHandler(handler);

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseFormUrlEncodedHandler(this RestClientFactoryConfig config)
        {
            DelegatingHandler HandlerFactory() => new FormUrlEncodedHandler();

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseMultipartFormDataHandler(this RestClientFactoryConfig config)
        {
            DelegatingHandler HandlerFactory() => new MultipartFormDataHandler();

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseRetryHandler(this RestClientFactoryConfig config)
        {
            DelegatingHandler HandlerFactory() => new RetryHandler();

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseRetryHandler(this RestClientFactoryConfig config, int maxRetries, double waitToRetryInSeconds, double maxWaitToRetryInSeconds, RetryHandler.BackOffStrategy backOffStrategy)
        {
            DelegatingHandler HandlerFactory() => new RetryHandler(maxRetries, waitToRetryInSeconds, maxWaitToRetryInSeconds, backOffStrategy);

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }

        public static RestClientFactoryConfig UseTwitterHandler(this RestClientFactoryConfig config, string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            DelegatingHandler HandlerFactory() => new TwitterHandler(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);

            config.HttpClientBuilder.AddHttpMessageHandler(HandlerFactory);

            return config;
        }
        
        public static RestClientFactoryConfig UseCookieHandler(this RestClientFactoryConfig config)
        {
            return UseCookieHandler(config, new CookieContainer());
        }

        public static RestClientFactoryConfig UseCookieHandler(this RestClientFactoryConfig config, CookieContainer cookieContainer)
        {
            config.UseHttpClientHandler(httpClientHandler => { httpClientHandler.CookieContainer = cookieContainer; });

            DelegatingHandler HandlerFactory() => new CookieHandler();
            config.UseHandler(HandlerFactory);

            return config;
        }
    }
}
