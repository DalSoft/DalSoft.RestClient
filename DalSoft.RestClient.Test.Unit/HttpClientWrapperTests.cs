using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Test.Unit.TestModels;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit
{
    public class HttpClientWrapperTests
    {
        public const string BaseUrl = "http://test.test";

        [Test]
        public void Ctor_Parameterless_ShouldNullObjectDefaultHeadersAndConfig()
        {
            var httpClientWrapper = new HttpClientWrapper();
            var actualHandler = httpClientWrapper.GetHandler() as DefaultJsonHandler;

            Assert.That(httpClientWrapper.DefaultRequestHeaders, Is.Not.Null);
            Assert.IsInstanceOf<DefaultJsonHandler>(actualHandler); //set up by default config
        }

        [Test]
        public void Ctor_PassingDefaultHeaders_ShouldDefaultHeadersToHttpClient()
        {
            var httpClientWrapper = new HttpClientWrapper(new Dictionary<string, string> { { "header1", "value1"}, { "header2", "value2" } });
          
            Assert.That(httpClientWrapper.DefaultRequestHeaders.Count, Is.EqualTo(2));
        }

        [Test]
        public void Ctor_NullHttpMessageHandler_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(()=>new HttpClientWrapper(new Config((HttpMessageHandler)null)));;
        }

        [Test]
        public void Ctor_DelegatingHandlerWithNonNullInnerHandler_ThrowsArgumentException()
        {
            var delegatingHandler = new DelegatingHandlerWrapper((request, token, next) => next(request, token));
            delegatingHandler.InnerHandler = delegatingHandler;

            Assert.Throws<ArgumentException>(() => new HttpClientWrapper(new Config(delegatingHandler))); ;
        }

        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandler_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();

            var httpClientWrapper = new HttpClientWrapper(new Config(expectedHandler));

            var actualHandler = (httpClientWrapper.GetHandler() as DelegatingHandler)?.InnerHandler as HttpClientHandler;

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandlerAndDefaultRequestHeaders_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();

            var httpClientWrapper = new HttpClientWrapper(new Dictionary<string, string>(), new Config(expectedHandler));

            var actualHandler = (httpClientWrapper.GetHandler() as DelegatingHandler)?.InnerHandler as HttpClientHandler;

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Ctor_DoesNotMatterWhatPositionTheHttpClientHandlerIsInThePipeline_ItWillAlwaysBeLastInThePipeline()
        {
            var expectedHandler = new HttpClientHandler();

            var httpClientWrapper = new HttpClientWrapper(new Config(new UnitTestHandler(), new UnitTestHandler(), expectedHandler, new UnitTestHandler(), new UnitTestHandler()));

            var actualHandler = httpClientWrapper.GetHandler()
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
            var httpClientWrapper = new HttpClientWrapper(new Config(new UnitTestHandler(), new UnitTestHandler(), new UnitTestHandler(), new UnitTestHandler()));

            var actualHandler = httpClientWrapper.GetHandler()
                .CastHandler<DefaultJsonHandler>();
            
            Assert.IsInstanceOf<DefaultJsonHandler>(actualHandler);
        }

        [Test]
        public async Task Ctor_HttpMessageHandlers_AreInvokedInCorrectOrder()
        {
            var correctOrder = string.Empty;

            var httpClientWrapper = new HttpClientWrapper
            (
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
                )
            );

            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), null, new {});

            Assert.That(correctOrder, Is.EqualTo("123"));
        }

        [Test]
        public async Task Ctor_HttpMessageHandlerFuncs_AreInvokedInCorrectOrder()
        {
            var correctOrder = string.Empty;

            var httpClientWrapper = new HttpClientWrapper
            (
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
                )
            );

            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), null, new { });

            Assert.That(correctOrder, Is.EqualTo("123"));
        }

        [Test]
        public async Task Ctor_UseDefaultHandlersTrue_InvokesDefaultJsonHandler()
        {
            HttpRequestMessage invokedRequest = null;

            var httpClientWrapper = new HttpClientWrapper
            ( 
                new Config //Default ctor for Config sets UseDefaultHandlers to true
                (
                    new UnitTestHandler(request =>
                    {
                        invokedRequest = request;
                    })
                )
            );

            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), null, new { });

            Assert.That(invokedRequest?.Headers.Accept.FirstOrDefault()?.MediaType, Is.EqualTo(Config.JsonContentType));
        }

        [Test]
        public async Task Ctor_UseDefaultHandlersFalse_DoesNotInvokeDefaultJsonHandler()
        {
            HttpRequestMessage invokedRequest = null;

            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => { invokedRequest = request; }))
                {
                    UseDefaultHandlers = false
                }
            );

            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), null, new { });

            Assert.True(invokedRequest?.Headers.Accept.Count == 0);
        }


        [Test]
        public void Ctor_Timeout_ShouldPassTimeoutToHttpClient()
        {
            var timeout = TimeSpan.FromSeconds(3);
            var httpClientWrapper = new HttpClientWrapper(new Config { Timeout = timeout });

            var httpClient = httpClientWrapper.GetHttpClient();

            Assert.That(httpClient.Timeout, Is.EqualTo(timeout));
        }

        [Test]
        public void Ctor_MaxResponseContentBufferSize_ShouldPassMaxResponseContentBufferSizeToHttpClient()
        {
            const long maxResponseContentBufferSize = 100;
            var httpClientWrapper = new HttpClientWrapper(new Config { MaxResponseContentBufferSize = maxResponseContentBufferSize });

            var httpClient = httpClientWrapper.GetHttpClient();

            Assert.That(httpClient.MaxResponseContentBufferSize, Is.EqualTo(maxResponseContentBufferSize));
        }

        [Test]
        public async Task Send_PassingHeadersTheSameAsDefaultHeaders_DoesNotAddTheHeadersTwice()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "myheader1", "myheadervalue1" } }, 
                new Config(new UnitTestHandler(request => actualRequest = request)) {  UseDefaultHandlers = false }
            );
            
            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), new Dictionary<string, string> { { "myheader1", "myheadervalue1"} }, new { });

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Send_PassesDefaultHeaders_CorrectlyToHttpClient()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "myheader1", "myheadervalue1" } },
                new Config(new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false }
            );

            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), null, new { });

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Send_PassesParameters_CorrectlyToHttpClient()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false }
            );

            var payload = new User { id=1 };
            await httpClientWrapper.Send(HttpMethod.Get, new Uri(BaseUrl), new Dictionary<string, string> { { "myheader1", "myheadervalue1" } }, payload);

            Assert.That(actualRequest.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(actualRequest.RequestUri.AbsoluteUri, Is.EqualTo(BaseUrl + "/"));
            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
            Assert.That(actualRequest.GetContent(), Is.EqualTo(payload));
        }

        [Test]
        public void Dispose_WhenCalled_DisposesOfHttpClient()
        {
            var httpClientWrapper = new HttpClientWrapper();

            httpClientWrapper.Dispose();

            Assert.True((bool)TestHelper.GetPrivateField(httpClientWrapper.GetHttpClient(), "disposed"));
        }
    }
}
