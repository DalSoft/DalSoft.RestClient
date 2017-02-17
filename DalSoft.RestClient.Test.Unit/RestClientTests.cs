using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;

namespace DalSoft.RestClient.Test.Unit
{
    [TestFixture]
    public class RestClientTests
    {
        [Test]
        public async Task Query_ShouldSerializeObjectToQueryString()
        {
            const string baseUri = "http://test.test";
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage()}));

            dynamic client = new RestClient(mockHttpClient.Object, baseUri);
            await client.Query(new { Id = "test", another = 1 }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get, 
                It.Is<Uri>(__ => __ == new Uri(baseUri + "?Id=test&another=1")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }

        [Test]
        public async Task Query_ShouldSerializeArrayToQueryString()
        {
            const string baseUri = "http://test.test";
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, baseUri);
            await client.Query(new { variables = new[] { "one", "other" }, otherVar = "stillWorks" }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(baseUri + "?variables=one&variables=other&otherVar=stillWorks")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }

        [Test]
        public async Task ToString_NullContent_ReturnsEmptyString()
        {
            const string baseUri = "http://test.test";
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), null))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, baseUri);
            var result = await client.Get();

           Assert.That(result.ToString(), Is.EqualTo(string.Empty));
        }
    }
}
