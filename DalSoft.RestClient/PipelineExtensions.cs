using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using DalSoft.RestClient.DependencyInjection;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Serialization;
using Newtonsoft.Json;

namespace DalSoft.RestClient
{
    public static class PipelineExtensions
    {
#if NET8_0_OR_GREATER
        internal static IDictionary<string, object> GetStateBag(this HttpRequestMessage request) => request.Options; //HttpRequestMessage.Properties is obsolete on modern runtimes
#else
        internal static IDictionary<string, object> GetStateBag(this HttpRequestMessage request) => request.Properties;
#endif

        internal static void SetConfig(this HttpRequestMessage request, Config config)
        {
            request.GetStateBag()[Config.ConfigKey] = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static Config GetConfig(this HttpRequestMessage request)
        {
            return request.GetStateBag().TryGetValue(Config.ConfigKey, out var config) ? config as Config : null;
        }

        internal static void SetContent(this HttpRequestMessage request, object content)
        {
            request.GetStateBag()[Config.RequestContentKey] = content;
        }

        public static object GetContent(this HttpRequestMessage request)
        {
            return request.GetStateBag().TryGetValue(Config.RequestContentKey, out var content) ? content : null;
        }

        internal static void SetContentType(this HttpRequestMessage request, string contentType)
        {
            request.GetStateBag()[Config.RequestContentType] = contentType;
        }

        public static string GetContentType(this HttpRequestMessage request)
        {
            return request.GetStateBag().TryGetValue(Config.RequestContentType, out var contentType) ? contentType as string : null;
        }

        public static void ExpectJsonResponse(this HttpRequestMessage request, bool expectJson)
        {
            request.GetStateBag()[Config.ResponseIsJsonKey] = expectJson;
        }

        public static bool ExpectJsonResponse(this HttpRequestMessage request)
        {
            return request.GetStateBag().TryGetValue(Config.ResponseIsJsonKey, out var expectJson) && (expectJson as bool? ?? false);
        }

        internal static void SetCookieContainer(this HttpRequestMessage request, CookieContainer cookieContainer)
        {
            request.GetStateBag()[Config.CookieContainerKey] = cookieContainer;
        }

        public static CookieContainer GetCookieContainer(this HttpResponseMessage response)
        {
            return response.RequestMessage.GetStateBag().TryGetValue(Config.CookieContainerKey, out var cookieContainer) ? cookieContainer as CookieContainer : null;
        }

        public static Config SetJsonSerializerOptions(this Config config, JsonSerializerOptions jsonSerializerOptions)
        {
            config.JsonSerializer = new SystemTextJsonSerializer(jsonSerializerOptions);
            return config;
        }

        public static Config UseNewtonsoftJson(this Config config, JsonSerializerSettings jsonSerializerSettings = null)
        {
            config.JsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);
            return config;
        }

        public static Config UseNoDefaultHandlers(this Config config)
        {
            config.UseDefaultHandlers = false;
            return config;
        }

        public static Config UseHandler(this Config config, HttpMessageHandler handler)
        {
            var handlers = config.Pipeline.ToList(); // New list so we can validate before adding

            handlers.Add(handler);

            handlers.ValidatePipeline();

            config.Pipeline = handlers;

            return config;
        }

        public static Config UseHandler(this Config config, Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> handler)
        {
            return UseHandler(config, new DelegatingHandlerWrapper(handler));
        }

        [Obsolete("Use UseHttpClientHandler(Config, Action<HttpClientHandler>) overload instead")]
        public static Config UseHttpClientHandler(this Config config, HttpClientHandler handler)
        {
            return UseHandler(config, handler);
        }

        public static Config UseHttpClientHandler(this Config config, Action<HttpClientHandler> httpClientHandlerOptions)
        {
            if (!config.TryGetHttpClientHandler(out var httpClientHandler))
            {
                httpClientHandler = new HttpClientHandler();
                httpClientHandlerOptions(httpClientHandler);

                return UseHandler(config, httpClientHandler);
            }

            httpClientHandlerOptions(httpClientHandler);

            return config;
        }

        public static Config UseUnitTestHandler(this Config config)
        {
            return UseHandler(config, new UnitTestHandler());
        }

        public static Config UseUnitTestHandler(this Config config, Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            return UseHandler(config, new UnitTestHandler(handler));
        }

        public static Config UseUnitTestHandler(this Config config, Action<HttpRequestMessage> handler)
        {
            return UseHandler(config, new UnitTestHandler(handler));
        }

        public static Config UseFormUrlEncodedHandler(this Config config)
        {
            return UseHandler(config, new FormUrlEncodedHandler());
        }
        
        public static Config UseMultipartFormDataHandler(this Config config)
        {
            return UseHandler(config, new MultipartFormDataHandler());
        }

        public static Config UseRetryHandler(this Config config)
        {
            return UseHandler(config, new RetryHandler());
        }

        public static Config UseRetryHandler(this Config config, int maxRetries, double waitToRetryInSeconds, double maxWaitToRetryInSeconds, RetryHandler.BackOffStrategy backOffStrategy)
        {
            return UseHandler(config, new RetryHandler(maxRetries, waitToRetryInSeconds, maxWaitToRetryInSeconds, backOffStrategy));
        }

        public static Config UseTwitterHandler(this Config config, string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            return UseHandler(config, new TwitterHandler(consumerKey, consumerKeySecret, accessToken, accessTokenSecret));
        }

        public static Config UseCookieHandler(this Config config)
        {
            return UseCookieHandler(config, new CookieContainer());
        }

        public static Config UseCookieHandler(this Config config, CookieContainer cookieContainer)
        {
             UseHttpClientHandler(config,
                httpClientHandler => httpClientHandler.CookieContainer = cookieContainer);

             return UseHandler(config, new CookieHandler());
        }

        internal static void ValidatePipeline(this IEnumerable<HttpMessageHandler> pipeline)
        {
            if (pipeline.OfType<HttpClientHandler>().Count() > 1)
                throw new ArgumentException("The pipeline can only have one HttpClientHandler", nameof(pipeline));
        }

        internal static bool TryGetHttpClientHandler(this DelegatingHandler delegatingHandler, out HttpClientHandler httpClientHandler)
        {
            httpClientHandler = GetHttpClientHandler(delegatingHandler);

            return httpClientHandler != null;
        }

        private static HttpClientHandler GetHttpClientHandler(this DelegatingHandler handler)
        {
            switch (handler.InnerHandler)
            {
                case HttpClientHandlerWrapper httpClientHandlerWrapper:
                    return httpClientHandlerWrapper;
                case HttpClientHandler httpClientHandler:
                    return httpClientHandler;
                case DelegatingHandler delegatingHandler:
                    return GetHttpClientHandler(delegatingHandler);
                default:
                    return null;
            }
        }
    }
}
