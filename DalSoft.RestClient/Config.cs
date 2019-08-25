using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.DependencyInjection;
using DalSoft.RestClient.Handlers;
using Newtonsoft.Json;

namespace DalSoft.RestClient
{
    public class Config
    {
        internal static readonly string JsonMediaType = "application/json";
        internal static readonly string RequestContentKey = "DalSoft.RestClient.RequestContentKey";
        internal static readonly string RequestContentType = "DalSoft.RestClient.RequestContentType";
        internal static readonly string ResponseIsJsonKey = "DalSoft.RestClient.ResponseIsJsonKey";
        internal static readonly string ConfigKey = "DalSoft.RestClient.Config";
        internal static readonly string CookieContainerKey = "DalSoft.RestClient.CookieContainerKey";

        internal IEnumerable<HttpMessageHandler> Pipeline { get; set; }

        public TimeSpan Timeout { get; set; }
        public long MaxResponseContentBufferSize { get; set; }
        public bool UseDefaultHandlers { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        
        public Config() : this((HttpMessageHandler[])null) { }

        public Config(params Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>>[] handlers) :
            this(handlers?.Select(handler => new DelegatingHandlerWrapper(handler)).ToArray()) {  }
        
        public Config(params HttpMessageHandler[] pipeline)
        {
            pipeline = pipeline ?? new HttpMessageHandler[] {};
            pipeline.ValidatePipeline();

            var handlers = pipeline.ToList();
            handlers.Insert(0, new DefaultJsonHandler(this));

            Pipeline = handlers;
            Timeout = TimeSpan.FromSeconds(100.0);        //Same default as HttpClient
            MaxResponseContentBufferSize = int.MaxValue;  //Same default as HttpClient
            UseDefaultHandlers = true;
        }

        internal bool TryGetHttpClientHandler(out HttpClientHandler httpClientHandler)
        {
            httpClientHandler = Pipeline.OfType<HttpClientHandler>().SingleOrDefault();
            return httpClientHandler !=null;
        }

        internal HttpMessageHandler CreatePipeline()
        {
            var allHandlers = Pipeline.Except(Pipeline.OfType<HttpClientHandler>());
            HttpMessageHandler primaryHandler = Pipeline.OfType<HttpClientHandler>().SingleOrDefault() ?? new HttpClientHandler();

            foreach (var handler in allHandlers.Reverse())
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(Pipeline), "Delegating Handlers Contains null Item");

                if (!(handler is DelegatingHandler delegatingHandler))
                    delegatingHandler = new HttpMessageHandlerToDelegatingHandler(handler);
                else
                {
                    if (delegatingHandler.InnerHandler != null)
                        throw new ArgumentException("Delegating Handlers Has non null Inner Handler");

                    delegatingHandler.InnerHandler = primaryHandler;
                }

                primaryHandler = delegatingHandler;
            }

            return primaryHandler;
        }

        
    }   
}
