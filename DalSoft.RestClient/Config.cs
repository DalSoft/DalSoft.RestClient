using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;

namespace DalSoft.RestClient
{
    public class Config
    {
        internal static readonly string JsonContentType = "application/json";

        public static readonly string Contentkey = "DalSoft.RestClient.Content";
        
        public Config() : this((Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task <HttpResponseMessage>>) null) { }

        public Config(params Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>>[] handlers) :
            this(handlers.Select(handler => new DelegatingHandlerWrapper(handler)).ToArray()) {  }

        public Config(params HttpMessageHandler[] pipeline)
        {
            if (pipeline.OfType<HttpClientHandler>().Count() > 1)
                throw new ArgumentException("The pipeline can only have one HttpClientHandler", nameof(pipeline));

            var handlers = pipeline.ToList();
            handlers.Insert(0, new DefaultJsonHandler(this));

            Pipeline = handlers;
            Timeout = TimeSpan.FromSeconds(100.0);        //Same default as HttpClient
            MaxResponseContentBufferSize = int.MaxValue;  //Same default as HttpClient
            UseDefaultHandlers = true;
        }

        public TimeSpan Timeout { get; set; }
        public long MaxResponseContentBufferSize { get; set; }
        public IEnumerable<HttpMessageHandler> Pipeline { get; set; }
        public bool UseDefaultHandlers { get; set; }
    }   
}
