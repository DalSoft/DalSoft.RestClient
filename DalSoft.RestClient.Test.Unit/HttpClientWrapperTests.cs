using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit
{
    public class HttpClientWrapperTests
    {
        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandler_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();

            var restClient = new RestClient("https://www.google.com", new Config(expectedHandler));

            var actualHandler = (restClient.GetHandler() as DelegatingHandler)?.InnerHandler as HttpClientHandler;

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandlerAndDefaultRequestHeaders_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();

            var restClient = new RestClient("https://www.google.com", new Dictionary<string, string>(), new Config(expectedHandler));

            var actualHandler = (restClient.GetHandler() as DelegatingHandler)?.InnerHandler as HttpClientHandler;

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Ctor_DoesNotMatterWhatPositionTheHttpClientHandlerIsInThePipeline_ItWillAlwaysBeLastInThePipeline()
        {
            var expectedHandler = new HttpClientHandler();

            var restClient = new RestClient("https://www.google.com", new Config(new UnitTestHandler(), new UnitTestHandler(), expectedHandler, new UnitTestHandler(), new UnitTestHandler()));

            var actualHandler = restClient.GetHandler()
                .CastHandler<DefaultJsonHandler>().InnerHandler
                .CastHandler<UnitTestHandler>().InnerHandler
                .CastHandler<UnitTestHandler>().InnerHandler
                .CastHandler<UnitTestHandler>().InnerHandler
                .CastHandler<UnitTestHandler>().InnerHandler
                .CastHandler<HttpClientHandler>();
            
            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        
        [Test]
        public void Ctor_DefaultJsonHandler_WillAlwaysBeFirstInThePipeline()
        {
            var restClient = new RestClient("https://www.google.com", new Config(new UnitTestHandler(), new UnitTestHandler(), new UnitTestHandler(), new UnitTestHandler()));

            var actualHandler = restClient.GetHandler()
                .CastHandler<DefaultJsonHandler>();
            
            Assert.IsInstanceOf<DefaultJsonHandler>(actualHandler);
        }

        [Test]
        public async Task Ctor_HttpMessageHandlers_AreInvokedInCorrectOrder()
        {
            var correctOrder = string.Empty;

            dynamic restClient = new RestClient("https://www.google.com", new Dictionary<string, string>(),
            new Config //Default ctor for Config sets UseDefaultHandlers to true
            (
                new DelegatingHandlerWrapper((request, token, next) => 
                {
                    correctOrder += "1";
                    return next(request, token);
                }),
                new DelegatingHandlerWrapper((request, token, next) =>
                {
                    correctOrder += "2";
                    return next(request, token);
                }),
                new DelegatingHandlerWrapper((request, token, next) =>
                {
                    correctOrder += "3";
                    return next(request, token);
                }),
                new UnitTestHandler() //end pipeline
            ));

            await restClient.Get();

            Assert.That(correctOrder, Is.EqualTo("123"));
        }

        [Test]
        public async Task Ctor_HttpMessageHandlerFuncs_AreInvokedInCorrectOrder()
        {
            var correctOrder = string.Empty;

            dynamic restClient = new RestClient("https://www.google.com", new Dictionary<string, string>(),
            new Config //Default ctor for Config sets UseDefaultHandlers to true
            (
                (request, token, next) =>
                {
                    correctOrder += "1";
                    return next(request, token);
                },
                (request, token, next) =>
                {
                    correctOrder += "2";
                    return next(request, token);
                },
                (request, token, next) =>
                {
                    correctOrder += "3";
                    return next(request, token);
                },
                (request, token, next) => Task.FromResult(new HttpResponseMessage { RequestMessage = request }) //end pipeline
            ));

            await restClient.Get();

            Assert.That(correctOrder, Is.EqualTo("123"));
        }

        [Test]
        public async Task Ctor_UseDefaultHandlersTrue_InvokesDefaultJsonHandler()
        {
            HttpRequestMessage invokedRequest = null;

            dynamic restClient = new RestClient("https://www.google.com", new Dictionary<string, string>(), 
            new Config //Default ctor for Config sets UseDefaultHandlers to true
            (
                new UnitTestHandler(request =>
                {
                    invokedRequest = request;
                })
            ));
            
            await restClient.Get();

            Assert.That(invokedRequest?.Headers.Accept.FirstOrDefault()?.MediaType, Is.EqualTo(Config.JsonContentType));
        }

        [Test]
        public async Task Ctor_UseDefaultHandlersFalse_DoesNotInvokeDefaultJsonHandler()
        {
            HttpRequestMessage invokedRequest = null;

            dynamic restClient = new RestClient("https://www.google.com", new Dictionary<string, string>(),
            new Config
            (
                new UnitTestHandler(request =>
                {
                    invokedRequest = request;
                })
            ) { UseDefaultHandlers = false });

            await restClient.Get();

            Assert.True(invokedRequest?.Headers.Accept.Count == 0);
        }


        [Test]
        public void Ctor_Timeout_ShouldPassTimeoutToHttpClient()
        {
            var timeout = TimeSpan.FromSeconds(3);
            var restClient = new RestClient("https://www.google.com", new Config { Timeout = timeout });

            var httpClient = restClient.GetHttpClient();

            Assert.That(httpClient.Timeout, Is.EqualTo(timeout));
        }

        [Test]
        public void Ctor_MaxResponseContentBufferSize_ShouldPassMaxResponseContentBufferSizeToHttpClient()
        {
            const long maxResponseContentBufferSize = 100;
            var restClient = new RestClient("https://www.google.com", new Config { MaxResponseContentBufferSize = maxResponseContentBufferSize });

            var httpClient = restClient.GetHttpClient();

            Assert.That(httpClient.MaxResponseContentBufferSize, Is.EqualTo(maxResponseContentBufferSize));
        }
    }
}
