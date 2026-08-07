using System;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Serialization;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Serialization
{
    [TestFixture]
    public class NewtonsoftJsonSerializerTests
    {
        private readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();

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
        public void TryParse_SingleQuotedJson_ReturnsTrue()
        {
            //Json.NET leniency is preserved for the fallback
            var isValidJson = _serializer.TryParse("{ 'id': 1 }", out var result);

            Assert.True(isValidJson);
            Assert.That(result.Kind, Is.EqualTo(JsonNodeKind.Object));
        }

        [Test]
        public void GetValue_JsonPrimitives_ReturnsExpectedClrTypes()
        {
            _serializer.TryParse("{ \"id\": 1, \"price\": 1.5, \"active\": true, \"date\": \"2020-01-01T00:00:00Z\" }", out var result);

            Assert.That(result.GetMember("id").GetValue(), Is.TypeOf<long>().And.EqualTo(1));
            Assert.That(result.GetMember("price").GetValue(), Is.TypeOf<double>().And.EqualTo(1.5));
            Assert.That(result.GetMember("active").GetValue(), Is.TypeOf<bool>().And.EqualTo(true));
            Assert.That(result.GetMember("date").GetValue(), Is.TypeOf<DateTime>()); //Json.NET DateTime sniffing is preserved for the fallback
        }

        [Test]
        public async Task Get_UsingNewtonsoftJsonWithDynamicAccess_ReturnsDynamicCorrectly()
        {
            var config = new Config()
                .UseNewtonsoftJson()
                .UseUnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent("[ { \"id\": 1, \"name\": \"Leanne Graham\" }, { \"id\": 2, \"name\": \"Ervin Howell\" } ]")
                });

            dynamic restClient = new RestClient("http://test.test", config);

            var users = await restClient.Users.Get();

            Assert.That(users[0].id, Is.EqualTo(1));
            Assert.That(users[1].name, Is.EqualTo("Ervin Howell"));

            var count = 0;
            foreach (var user in users)
            {
                count++;
                Assert.NotNull(user.name);
            }

            Assert.That(count, Is.EqualTo(2));
        }
    }
}
