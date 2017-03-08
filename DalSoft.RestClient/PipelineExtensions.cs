using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;

namespace DalSoft.RestClient
{
    public static class PipelineExtensions
    {
        internal static void SetContent(this HttpRequestMessage request, object content)
        {
            request.Properties[Config.RequestContentKey] = content;
        }

        public static object GetContent(this HttpRequestMessage request)
        {
            return request.Properties.ContainsKey(Config.RequestContentKey) ? request.Properties[Config.RequestContentKey] : null;
        }

        internal static void SetContentType(this HttpRequestMessage request, string contentType)
        {
            request.Properties[Config.RequestContentType] = contentType;
        }

        public static string GetContentType(this HttpRequestMessage request)
        {
            return request.Properties.ContainsKey(Config.RequestContentType) ? request.Properties[Config.RequestContentType] as string : null;
        }

        public static void ExpectJsonResponse(this HttpRequestMessage request, bool expectJson)
        {
            request.Properties[Config.ResponseIsJsonKey] = expectJson;
        }

        public static bool ExpectJsonResponse(this HttpRequestMessage request)
        {
            return request.Properties.ContainsKey(Config.ResponseIsJsonKey) && (request.Properties[Config.ResponseIsJsonKey] as bool? ?? false);
        }
        
        public static Config UseNoDefaultHandlers(this Config config)
        {
            config.UseDefaultHandlers = false;
            return config;
        }

        public static Config UseHandler(this Config config, HttpMessageHandler handler)
        {
            var handlers = config.Pipeline.ToList();

            handlers.Add(handler);

            handlers.ValidatePipeline();

            config.Pipeline = handlers;

            return config;
        }

        public static Config UseHandler(this Config config, Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> handler)
        {
            return UseHandler(config, new DelegatingHandlerWrapper(handler));
        }

        public static Config UseHttpClientHandler(this Config config, HttpClientHandler handler)
        {
            return UseHandler(config, handler);
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

        internal static void ValidatePipeline(this IEnumerable<HttpMessageHandler> pipeline)
        {
            if (pipeline.OfType<HttpClientHandler>().Count() > 1)
                throw new ArgumentException("The pipeline can only have one HttpClientHandler", nameof(pipeline));
        }
    }
}
