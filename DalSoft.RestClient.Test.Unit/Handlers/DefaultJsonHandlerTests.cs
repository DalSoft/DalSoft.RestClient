using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    [TestFixture]
    public class DefaultJsonHandlerTests
    {
        public const string BaseUrl = "http://test.test";

        [Test]
        public void Ctor_NullConfig_CreatesNullObject()
        {
            var handler = new DefaultJsonHandler(null);
            
            Assert.NotNull(TestHelper.GetPrivateField(handler, "_config"));
        }
        
        [Test]
        public async Task Send_NoDefaultHandlersTrue_DoesNothingExceptCallNextHandler()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config()
                    .UseUnitTestHandler(request => actualRequest = request)
                    .UseNoDefaultHandlers()
            );

            var response = await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new{});

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(0));
            Assert.That(actualRequest.Content, Is.Null);
        }


        [Test]
        public async Task Send_NullRequestContent_CorrectReturnsResponse()
        {
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new {hello = "world"}))
                }))
            );

            var response = await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, null);

            Assert.That(response.Content.ReadAsStringAsync().Result, Does.Contain("world"));
        }

        [Test]
        public async Task Send_DoNotSetContentType_SetsContentTypeToJson()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new { });

            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo(Config.JsonMediaType));
        }

        [Test]
        public async Task Send_DoNotSetAcceptHeader_SetsAcceptHeaderToJson()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new { });

            Assert.That(actualRequest.Headers.Accept.First().MediaType, Is.EqualTo(Config.JsonMediaType));
        }

        [Test]
        public async Task Send_ByDefault_SetsExpectJsonResponse()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, null);

            Assert.That(actualRequest.ExpectJsonResponse(), Is.True);
        }

        [Test]
        public async Task Send_PassesContentType_CorrectlyToHttpClient()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }, new {});

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public async Task Send_PassesContentTypeUsingDefaultHeaders_CorrectlyToHttpClient()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } },
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new {});

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public async Task Send_PassContentTypeAndTheSameContentTypeUsingDefaultHeaders_CorrectlyToHttpClient()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } },
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }, new{});

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public async Task Send_PassContentTypeAndContentTypeUsingDefaultHeadersIsDifferent_HttpClientUsesContentTypePassedToSend()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "Content-Type", "application/text" } },
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }, new {});

            Assert.That(actualRequest.Headers.Count(), Is.EqualTo(1));
            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo("application/x-www-form-urlencoded"));
        }
    }
}
