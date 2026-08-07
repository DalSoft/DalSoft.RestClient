using System.Net.Http;
using System.Net.Http.Headers;
using DalSoft.RestClient.Serialization;
using DalSoft.RestClient.Test.Unit.TestData.Models;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Serialization
{
    [TestFixture]
    public class SystemTextJsonSerializerTests
    {
        private readonly SystemTextJsonSerializer _serializer = new SystemTextJsonSerializer();

        [Test]
        public void TryParse_EmptyString_ReturnsTrueWithNullResult()
        {
            var isValidJson = _serializer.TryParse(string.Empty, out var result);

            Assert.True(isValidJson);
            Assert.Null(result);
        }

        [Test]
        public void TryParse_InvalidJson_ReturnsFalse()
        {
            var isValidJson = _serializer.TryParse("<html>not json</html>", out var result);

            Assert.False(isValidJson);
            Assert.Null(result);
        }

        [Test]
        public void TryParse_JsonWithTrailingCommasAndComments_ReturnsTrue()
        {
            //Real world systems are less than perfect, we accept trailing commas and comments by default
            var isValidJson = _serializer.TryParse("{ /* comment */ \"id\": 1, }", out var result);

            Assert.True(isValidJson);
            Assert.That(result.Kind, Is.EqualTo(JsonNodeKind.Object));
        }

        [Test]
        public void GetMember_MissingOrWrongCaseMember_ReturnsNull()
        {
            _serializer.TryParse("{ \"id\": 1 }", out var result);

            Assert.Null(result.GetMember("missing"));
            Assert.Null(result.GetMember("Id")); //Case sensitive just like System.Text.Json
        }

        [Test]
        public void GetValue_JsonPrimitives_ReturnsExpectedClrTypes()
        {
            _serializer.TryParse("{ \"id\": 1, \"price\": 1.5, \"active\": true, \"date\": \"2020-01-01T00:00:00Z\" }", out var result);

            Assert.That(result.GetMember("id").GetValue(), Is.TypeOf<long>().And.EqualTo(1));
            Assert.That(result.GetMember("price").GetValue(), Is.TypeOf<double>().And.EqualTo(1.5));
            Assert.That(result.GetMember("active").GetValue(), Is.TypeOf<bool>().And.EqualTo(true));
            Assert.That(result.GetMember("date").GetValue(), Is.TypeOf<string>().And.EqualTo("2020-01-01T00:00:00Z")); //Unlike Json.NET no DateTime sniffing, strings stay strings
        }

        [Test]
        public void Cast_ResponseWithNoConfigSet_DeserializesUsingDefaultSerializer()
        {
            //Regression test - casting a response created outside the pipeline (no config in the request state bag) shouldn't throw a NullReferenceException
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            dynamic responseObject = new RestClientResponseObject(new HttpResponseMessage { RequestMessage = request }, "{ \"id\": 1 }");

            User user = responseObject;

            Assert.That(user.id, Is.EqualTo(1));
        }
    }
}
