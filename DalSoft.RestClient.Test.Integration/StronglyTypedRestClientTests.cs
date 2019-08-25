using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Test.Integration.TestModels;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Integration
{
    [TestFixture]
    public class StronglyTypedRestClientTests
    {
        private const string BaseUri = "http://jsonplaceholder.typicode.com";

        [Test]
        public async Task Get_StronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri); // Strongly typed RestClient not dynamic

            User user = await client
                .Resource("users")
                .Resource("1")
                .Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_StronglyTypedRestClientResourceExtension_CorrectlyInvokesMethod()
        {
            var client = new RestClient(BaseUri);

            User user = await client // Strongly typed RestClient extension method on resource
                .Users(1) // extension method
                .Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_StronglyTypedRestClientResourceAndVerbExtension_CorrectlyInvokesMethod()
        {
            var client = new RestClient(BaseUri);

            var user = await client.GetUserById(1); // Strongly typed RestClient extension method resource and verb

            Assert.AreEqual(1, user.id);
        }


        [Test]
        public async Task Get_StronglyTypedRestClientResourceAndVerbExtension_CorrectlyInvokesUsingDynamicMethods()
        {
            var client = new RestClient(BaseUri);

            var user = await client.GetUserByIdDynamic(1); // dynamic extension method full verb

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_StronglyTypedRestClient_CanBeCastBackToDynamic()
        {
            var client = new RestClient(BaseUri);

            dynamic testBackAsDynamic = client; // Strongly typed RestClient cast back to dynamic
            var user = await testBackAsDynamic.Users(1).Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Post_StronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri);

            User user = await client
                .Resource("users")
                .Post(new User { name = "DalSoft" });

            Assert.AreEqual("DalSoft", user.name);
        }


        [Test]
        public async Task PostAnonymousObject_StronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri);

            User user = await client
                .Resource("users")
                .Post(new { name = "DalSoft" });

            Assert.AreEqual("DalSoft", user.name);
        }

        [Test]
        public async Task Get_StronglyTypedRestClient_CanBeCastIRestClient()
        {
            dynamic restClient = new RestClient(BaseUri);

            IRestClient http = restClient;

            User user = await http.Resource("users/1").Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task StronglyTypedRestClient_HttpClientMember_IsAUsableHttpClient()
        {
            var client = new RestClient(BaseUri);

            var httpClientResponseString = await client.HttpClient.GetStringAsync(client.BaseUri + "/users/1");

            Assert.True(httpClientResponseString.Contains("Leanne Graham"));
        }

        [Test]
        public async Task StronglyTypedRestClient_CastToHttpResponseMessage_IsCastToHttpResponseMessage()
        {
            var client = new RestClient(BaseUri); // Strongly typed RestClient not dynamic

            HttpResponseMessage httpResponseMessage = await client //Strongly typed RestClient then cast to HttpResponseMessage
                .Resource("users")
                .Resource("1")
                .Get();

            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Test]
        public async Task StronglyTypedRestClient_SetCookiesUsingCookieHandler_CorrectlySetsCookie()
        {
            var restClient = new RestClient("https://httpbin.org/cookies/set?testcookie=darran", new Config()
                .UseCookieHandler());

            HttpResponseMessage response = await restClient.Get();

            Assert.AreEqual("darran", response.GetCookieContainer()?.GetCookies(new Uri("https://httpbin.org"))["testcookie"]?.Value);
        }
    }
}
