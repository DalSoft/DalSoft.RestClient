using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Test.Unit
{
    [TestFixture]
    public class RestClientTests
    {
        [Test]
        public async Task Query_ShouldSerializeObjectToQueryString()
        {
            // Arrange
            var spy = new HttpClientWrapperSpy();
            var baseUri = "http://test.test/";

            dynamic client = new RestClient(spy, baseUri);

            // Act
            var result = await client.Query(new { Id = "test", another = 1 }).Get();

            // Assert
            Assert.AreEqual(new Uri(baseUri + "?Id=test&another=1"), spy.Uri);
        }

        [Test]
        public async Task Query_ShouldSerializeArrayToQueryString()
        {
            // Arrange
            var spy = new HttpClientWrapperSpy();
            var baseUri = "http://test.test/";

            dynamic client = new RestClient(spy, baseUri);

            // Act
            var result = await client.Query(new
            {
                variables = new[] { "one", "other" },
                otherVar = "stillWorks"
            }).Get();

            // Assert
            Assert.AreEqual(new Uri(baseUri + "?variables=one&variables=other&otherVar=stillWorks"), spy.Uri);
        }
    }
}
