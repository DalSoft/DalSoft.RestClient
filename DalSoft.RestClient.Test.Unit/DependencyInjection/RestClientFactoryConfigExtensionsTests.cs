using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.DependencyInjection;
using DalSoft.RestClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.DependencyInjection
{
    [TestFixture]
    public class RestClientFactoryConfigExtensionsTests
    {
        private const string Name = "MyClient1";

        [Test]
        public void SetJsonSerializerSettings_WhenCalled_SetsJsonSerializerSettings()
        {
            var services = new ServiceCollection();
            var expected = new JsonSerializerSettings();

            var config = services.AddRestClient(Name, "http://dalsoft.co.uk")
                .SetJsonSerializerSettings(expected);

            Assert.AreSame(expected, config.JsonSerializerSettings);
        }

        [Test]
        public void UseNoDefaultHandlers_WhenCalled_SetsUseDefaultHandlersToFalse()
        {
            var services = new ServiceCollection();

            var config = services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseNoDefaultHandlers();

            Assert.That(config.UseDefaultHandlers, Is.False);
        }
        
        [Test]
        public void UseHandler_AddHandlers_CorrectlyAddsHandlers()
        {
            bool formUrlEncodedHandlerCalled = false, unitTestHandlerCalled = false;
            var services = new ServiceCollection();
            
            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseHandler(() =>
                {
                    formUrlEncodedHandlerCalled = true;
                    return new FormUrlEncodedHandler();
                })
                .UseHandler(() =>
                {
                    unitTestHandlerCalled = true;
                    return new UnitTestHandler();
                });

            services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name);

            Assert.True(formUrlEncodedHandlerCalled);
            Assert.True(unitTestHandlerCalled);
        }

        [Test]
        public async Task UseHandler_AddHandlersUsingFunc_CorrectlyAddsHandlers()
        {
            var handlerCalled = false;
            async Task<HttpResponseMessage> Handler(HttpRequestMessage request, CancellationToken token, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> next)
            {
                handlerCalled = true;

                return new HttpResponseMessage();
            }

            var services = new ServiceCollection();
            
            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseNoDefaultHandlers()
                .UseHandler(Handler);

            dynamic restClient = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name);
            await restClient.Get();
            
            Assert.True(handlerCalled);
        }
        
        [Test]
        public void UseHttpClientHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var httpClientHandlerCalled = false;
            var services = new ServiceCollection();

            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseHttpClientHandler(() =>
                {
                    httpClientHandlerCalled = true;
                    return new HttpClientHandler();
                });

            services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name);

            Assert.True(httpClientHandlerCalled);
        }

        [Test]
        public void UseUnitTestHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var services = new ServiceCollection();
            
            services.AddRestClient(Name, "http://dalsoft.co.uk")
               .UseUnitTestHandler();

            var httpClientFactoryOptions = services.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count, Is.EqualTo(2)); //DefaultJsonHandler & UnitTestHandler
        }

        [Test]
        public async Task UseUnitTestHandler_AddHandler_CorrectlyAddHandler()
        {
            var unitTestHandlerCalled = false;
            var services = new ServiceCollection();

            HttpResponseMessage Handler(HttpRequestMessage request)
            {
                unitTestHandlerCalled = true;
                return new HttpResponseMessage();
            }

            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseUnitTestHandler(Handler);

            dynamic restClient = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name);

            await restClient.Get();

            Assert.True(unitTestHandlerCalled);
        }

        [Test]
        public void UseFormUrlEncodedHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var services = new ServiceCollection();
            
            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseFormUrlEncodedHandler();

            var httpClientFactoryOptions = services.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count, Is.EqualTo(2)); //DefaultJsonHandler & UseFormUrlEncodedHandler
        }

        [Test]
        public void UseMultipartFormDataHandler_AddHandlers_CorrectlyAddHandlers()
        {
            var services = new ServiceCollection();
            
            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseMultipartFormDataHandler();

            var httpClientFactoryOptions = services.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count, Is.EqualTo(2)); //DefaultJsonHandler & UseMultipartFormDataHandler
        }

        [Test]
        public void UseRetryHandler_ParameterLess_CorrectlyAddHandlers()
        {
            const int defaultMaxRetries = 3;
            const double defaultWaitToRetryInSeconds = 1.44;
            const int defaultMaxWaitToRetryInSeconds = 10;
            const RetryHandler.BackOffStrategy defaultBackOffStrategy = RetryHandler.BackOffStrategy.Exponential;

            var services = new ServiceCollection();

            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseRetryHandler();
            
            var httpClientFactoryOptions = services.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count, Is.EqualTo(2)); //DefaultJsonHandler & UseRetryHandler

            //TODO: How to get the underline handler from the IHttpClientFactory
            //Assert.That(retryHandler?.MaxRetries, Is.EqualTo(defaultMaxRetries));
            //Assert.That(retryHandler?.WaitToRetryInSeconds, Is.EqualTo(defaultWaitToRetryInSeconds));
            //Assert.That(retryHandler?.MaxWaitToRetryInSeconds, Is.EqualTo(defaultMaxWaitToRetryInSeconds));
            //Assert.That(retryHandler?.CurrentBackOffStrategy, Is.EqualTo(defaultBackOffStrategy));
        }

        [Test]
        public void UseTwitterHandler_AddHandlers_CorrectlyAddHandlers()
        {
           var services = new ServiceCollection();

            services.AddRestClient(Name, "http://dalsoft.co.uk")
                .UseTwitterHandler(consumerKey:"consumerKey", consumerKeySecret:"consumerKeySecret", accessToken:"accessToken", accessTokenSecret:"accessTokenSecret");
            
            var httpClientFactoryOptions = services.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count, Is.EqualTo(2)); //DefaultJsonHandler & UseTwitterHandler
         }
    }
}
