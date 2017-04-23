using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Extensions
{
    [TestFixture]
    public class PipelineExtensionsTests
    {
        [Test]
        public void UseNoDefaultHandlers_WhenCalled_SetsUseDefaultHandlersToFalse()
        {
            var config = new Config()
                .UseNoDefaultHandlers();

            Assert.That(config.UseDefaultHandlers, Is.False);
        }

        [Test]
        public void UseHandler_AddHttpClientHandlerMoreThanOnce_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>
            (() =>
                new Config()
                    .UseHandler(new HttpClientHandler())
                    .UseHandler(new HttpClientHandler())
            );
        }

        [Test]
        public void UseHandler_AddHandlers_CorrectlyAddsHandlers()
        {
            var config = new Config()
                .UseHandler(new HttpClientHandler())
                .UseHandler(new UnitTestHandler());

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3));
            Assert.That(config.Pipeline.ElementAt(0), Is.InstanceOf<DefaultJsonHandler>());
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<HttpClientHandler>());
            Assert.That(config.Pipeline.ElementAt(2), Is.InstanceOf<UnitTestHandler>());
        }

        [Test]
        public void UseHandler_AddHandlersUsingFunc_CorrectlyAddsHandlers()
        {
            Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> 
                handler = (request, token, next) => next(request, token);

            var config = new Config()
                .UseHandler(new HttpClientHandler())
                .UseHandler(handler);

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3));
            Assert.That(config.Pipeline.ElementAt(2), Is.InstanceOf<DelegatingHandler>());
        }

        [Test]
        public void UseHandler_AddHandlersAndWithCtor_CorrectlyAddsHandlers()
        {
            var config = new Config(new UnitTestHandler())
                .UseHandler(new HttpClientHandler())
                .UseHandler(new UnitTestHandler());

            Assert.That(config.Pipeline.Count(), Is.EqualTo(4));
            Assert.That(config.Pipeline.ElementAt(0), Is.InstanceOf<DefaultJsonHandler>());
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<UnitTestHandler>());
            Assert.That(config.Pipeline.ElementAt(2), Is.InstanceOf<HttpClientHandler>());
            Assert.That(config.Pipeline.ElementAt(3), Is.InstanceOf<UnitTestHandler>());
        }

        [Test]
        public void UseHttpClientHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var config = new Config()
                .UseHttpClientHandler(new HttpClientHandler())
                .UseHandler(new UnitTestHandler());

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3));
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<HttpClientHandler>());
        }

        [Test]
        public void UseUnitTestHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var config = new Config()
                .UseHttpClientHandler(new HttpClientHandler())
                .UseUnitTestHandler();

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3));
            Assert.That(config.Pipeline.ElementAt(2), Is.InstanceOf<UnitTestHandler>());
        }

        [Test]
        public void UseUnitTestHandler_AddHandler_CorrectlyAddHandler()
        {
            Func<HttpRequestMessage, HttpResponseMessage> handler = request => new HttpResponseMessage();
            var config = new Config()
                .UseUnitTestHandler(handler);
            
            Assert.That(config.Pipeline.Count(), Is.EqualTo(2));
            Assert.That(TestHelper.GetPrivateField((UnitTestHandler)config.Pipeline.ElementAt(1), "_handler"), Is.EqualTo(handler));
        }

        [Test]
        public void UseFormUrlEncodedHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var config = new Config()
                .UseFormUrlEncodedHandler();

            Assert.That(config.Pipeline.Count(), Is.EqualTo(2));
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<FormUrlEncodedHandler>());
        }

        [Test]
        public void UseMultipartFormDataHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var config = new Config()
                .UseMultipartFormDataHandler();

            Assert.That(config.Pipeline.Count(), Is.EqualTo(2));
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<MultipartFormDataHandler>());
        }

        [Test]
        public void UseRetryHandler_ParameterLess_CorrectlyAddHandlers()
        {
            const int defaultMaxRetries = 3;
            const int defaultWaitToRetryInSeconds = 2;
            const int defaultMaxWaitToRetryInSeconds = 10;
            const RetryHandler.BackOffStrategy defaultBackOffStrategy = RetryHandler.BackOffStrategy.Exponential;

            var config = new Config()
                .UseRetryHandler();
            var retryHandler = config.Pipeline.ElementAt(1) as RetryHandler;

            Assert.That(config.Pipeline.Count(), Is.EqualTo(2));
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<RetryHandler>());
            Assert.That(retryHandler?.MaxRetries, Is.EqualTo(defaultMaxRetries));
            Assert.That(retryHandler?.WaitToRetryInSeconds, Is.EqualTo(defaultWaitToRetryInSeconds));
            Assert.That(retryHandler?.MaxWaitToRetryInSeconds, Is.EqualTo(defaultMaxWaitToRetryInSeconds));
            Assert.That(retryHandler?.CurrentBackOffStrategy, Is.EqualTo(defaultBackOffStrategy));
        }

        [Test]
        public void UseRetryHandler_WithParameters_CorrectlyAddHandlers()
        {
            const int maxRetries = 6;
            const int waitToRetryInSeconds = 5;
            const int maxWaitToRetryInSeconds = 30;
            const RetryHandler.BackOffStrategy backOffStrategy = RetryHandler.BackOffStrategy.Linear;

            var config = new Config()
                .UseRetryHandler(maxRetries:maxRetries, waitToRetryInSeconds:waitToRetryInSeconds, maxWaitToRetryInSeconds:maxWaitToRetryInSeconds, backOffStrategy:backOffStrategy);
            var retryHandler = config.Pipeline.ElementAt(1) as RetryHandler;

            Assert.That(config.Pipeline.Count(), Is.EqualTo(2));
            Assert.That(config.Pipeline.ElementAt(1), Is.InstanceOf<RetryHandler>());
            Assert.That(retryHandler?.MaxRetries, Is.EqualTo(maxRetries));
            Assert.That(retryHandler?.WaitToRetryInSeconds, Is.EqualTo(waitToRetryInSeconds));
            Assert.That(retryHandler?.MaxWaitToRetryInSeconds, Is.EqualTo(maxWaitToRetryInSeconds));
            Assert.That(retryHandler?.CurrentBackOffStrategy, Is.EqualTo(backOffStrategy));
        }

        [Test]
        public void ExpectJsonResponse_StateBagPropertyNull_ReturnsFalse()
        {
            Assert.False(new HttpRequestMessage().ExpectJsonResponse());
        }

        [Test]
        public void ExpectJsonResponse_StateBagPropertyTrue_ReturnsTrue()
        {
            var request = new HttpRequestMessage();

            request.ExpectJsonResponse(true);

            Assert.True(request.ExpectJsonResponse());
        }
    }
}
