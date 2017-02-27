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

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new{});

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

            var response = await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, content:null); //null content

            Assert.That(response.Content.ReadAsStringAsync().Result, Does.Contain("world"));
        }

        [Test]
        public async Task Send_DoNotSetAcceptHeader_SetsAcceptHeaderToJson()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), requestHeaders:null, content:new {});

            Assert.That(actualRequest.Headers.Accept.First().MediaType, Is.EqualTo(Config.JsonMediaType));
        }

        [Test]
        public async Task Send_EmptyAcceptHeader_SetsAcceptHeaderToJson()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), requestHeaders:new Dictionary<string, string>(), content:new {});

            Assert.That(actualRequest.Headers.Accept.First().MediaType, Is.EqualTo(Config.JsonMediaType));
        }
        [Test]
        public async Task Send_PassingAcceptHeader_IsTheOnlyHeaderSet()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), new Dictionary<string, string> { {"Accept", "application/custom+accept"} }, new { });

            Assert.That(actualRequest.Headers.Accept.Count, Is.EqualTo(1));
            Assert.That(actualRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/custom+accept"));
        }

        [Test]
        public async Task Send_PassingAcceptHeaderUsingDefaultHeaders_IsTheOnlyHeaderSet()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Dictionary<string, string> { { "Accept", "application/custom+accept" } },
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new { });

            Assert.That(actualRequest.Headers.Accept.Count, Is.EqualTo(1));
            Assert.That(actualRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/custom+accept"));
        }

        [Test]
        public async Task Send_PassingPrimitiveToContent_ThrowsArgumentException()
        {
            const int primitiveContentToTest = 1000;

            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler())
            );
         
            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new { });

            Assert.ThrowsAsync<ArgumentException>(async () => await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, primitiveContentToTest));
        }

        [Test]
        public async Task Send_PassingStringToContent_ThrowsArgumentException()
        {
            const string stringContentToTest = "This is a string";

            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler())
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, new { });

            Assert.ThrowsAsync<ArgumentException>(async () => await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), null, stringContentToTest));
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
        public async Task Send_SupportedJsonContentType_SetsContentTypeToSupportedJsonType()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            var supportedJsonContentTypes = new[] { "application/json", "text/json", "application/json-patch+json" };

            foreach (var supportedJsonContentType in supportedJsonContentTypes)
            {
                await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), new Dictionary<string, string> { { "Content-Type", supportedJsonContentType } }, new { });
                Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo(supportedJsonContentType));
            }
        }

        [Test]
        public async Task Send_SupportedJsonContentType_CorrectSerializesContent()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new UnitTestHandler(request => actualRequest = request))
            );

            var supportedJsonContentTypes = new[] { "application/json", "text/json", "application/json-patch+json" };

            foreach (var supportedJsonContentType in supportedJsonContentTypes)
            {
                await httpClientWrapper.Send(HttpMethod.Post, new Uri(BaseUrl), 
                    new Dictionary<string, string> { { "Content-Type", supportedJsonContentType } }, new { hello = "world" });

                dynamic deserializedContent = JsonConvert.DeserializeObject<dynamic>(actualRequest.Content.ReadAsStringAsync().Result);
                Assert.That(deserializedContent.hello.Value, Is.EqualTo("world"));
            }
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
    }
}
